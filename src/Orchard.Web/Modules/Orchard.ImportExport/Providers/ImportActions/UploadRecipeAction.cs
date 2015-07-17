﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Environment.Features;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;
using Orchard.Mvc;
using Orchard.Recipes.Services;
using Orchard.UI.Notify;

namespace Orchard.ImportExport.Providers.ImportActions {
    public class UploadRecipeAction : ImportAction {
        private readonly IOrchardServices _orchardServices;
        private readonly IImportExportService _importExportService;
        private readonly ISetupService _setupService;
        private readonly ShellSettings _shellSettings;
        private readonly IFeatureManager _featureManager;
        private readonly IEnumerable<IRecipeExecutionStep> _recipeExecutionSteps;

        public UploadRecipeAction(
            IOrchardServices orchardServices,
            IImportExportService importExportService,
            ISetupService setupService,
            ShellSettings shellSettings,
            IFeatureManager featureManager,
            IEnumerable<IRecipeExecutionStep> recipeExecutionSteps) {

            _orchardServices = orchardServices;
            _importExportService = importExportService;
            _setupService = setupService;
            _shellSettings = shellSettings;
            _featureManager = featureManager;
            _recipeExecutionSteps = recipeExecutionSteps;
        }

        public override string Name { get { return "UploadRecipe"; } }

        public XDocument RecipeDocument { get; set; }
        public bool ResetSite { get; set; }
        public string SuperUserPassword { get; set; }

        public override dynamic BuildEditor(dynamic shapeFactory) {
            return UpdateEditor(shapeFactory, null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater) {
            var viewModel = new UploadRecipeViewModel {
                SuperUserName = _orchardServices.WorkContext.CurrentSite.SuperUser,
                RecipeExecutionSteps = _recipeExecutionSteps.Select(x => new RecipeExecutionStepViewModel {
                    Name = x.Name,
                    DisplayName = x.DisplayName,
                    Description = x.Description,
                    Editor = x.BuildEditor(shapeFactory)
                }).Where(x => x.Editor != null).ToList()
            };

            if (updater != null) {
                if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                    // Validate and read uploaded recipe file.
                    var request = _orchardServices.WorkContext.HttpContext.Request;
                    var file = request.Files["RecipeFile"];
                    var isValid = true;

                    ResetSite = viewModel.ResetSite;
                    SuperUserPassword = viewModel.SuperUserPassword;

                    if (file == null || file.ContentLength == 0) {
                        updater.AddModelError("RecipeFile", T("No recipe file selected."));
                        isValid = false;
                    }

                    if (ResetSite) {
                        if (String.IsNullOrWhiteSpace(viewModel.SuperUserPassword)) {
                            updater.AddModelError("SuperUserPassword", T("Please specify a new password for the super user."));
                            isValid = false;
                        }
                        else if (!String.Equals(viewModel.SuperUserPassword, viewModel.SuperUserPasswordConfirmation)) {
                            updater.AddModelError("SuperUserPassword", T("The passwords do not match."));
                            isValid = false;
                        }
                    }

                    var stepUpdater = new Updater(updater, secondHalf => String.Format("{0}.{1}", Prefix, secondHalf));

                    // Update the view model with non-roundtripped values.
                    viewModel.SuperUserName = _orchardServices.WorkContext.CurrentSite.SuperUser;
                    foreach (var stepViewModel in viewModel.RecipeExecutionSteps) {
                        var step = _recipeExecutionSteps.Single(x => x.Name == stepViewModel.Name);
                        stepViewModel.DisplayName = step.DisplayName;
                        stepViewModel.Description = step.Description;

                        // Update the step with posted values.
                        stepViewModel.Editor = step.UpdateEditor(shapeFactory, stepUpdater);
                    }

                    if (isValid) {
                        // Read recipe file.
                        RecipeDocument = XDocument.Parse(new StreamReader(file.InputStream).ReadToEnd());
                        var orchardElement = RecipeDocument.Element("Orchard");

                        // Update execution steps.
                        var executionStepNames = viewModel.RecipeExecutionSteps.Where(x => x.IsSelected).Select(x => x.Name);
                        var executionStepsQuery =
                            from name in executionStepNames
                            where orchardElement.Element(name) != null
                            let provider = _recipeExecutionSteps.SingleOrDefault(x => x.Name == name)
                            where provider != null
                            select provider;
                        var executionSteps = executionStepsQuery.ToArray();
                        foreach (var executionStep in executionSteps) {
                            var context = new UpdateRecipeExecutionStepContext {
                                RecipeDocument = RecipeDocument,
                                Step = orchardElement.Element(executionStep.Name)
                            };

                            // Give the execution step a chance to augment the recipe step before it will be scheduled.
                            executionStep.UpdateStep(context);
                        }
                    }
                }
            }

            return shapeFactory.EditorTemplate(TemplateName: "ImportActions/UploadRecipe", Model: viewModel, Prefix: Prefix);
        }

        public override void Execute(ImportActionContext context) {
            if (RecipeDocument == null)
                return;

            // Sets the request timeout to 10 minutes to give enough time to execute custom recipes.
            _orchardServices.WorkContext.HttpContext.Server.ScriptTimeout = 600;

            var executionId = ResetSite ? Setup() : ExecuteRecipe();

            if(executionId == null) {
                _orchardServices.Notifier.Warning(T("The recipe contained no steps. No work was scheduled."));
                return;
            }

            context.ActionResult = new RedirectToRouteResult(new RouteValueDictionary(new { action = "ImportResult", controller = "Admin", area = "Orchard.ImportExport", executionId = executionId }));
        }

        private string Setup() {
            var setupContext = new SetupContext {
                DropExistingTables = true,
                RecipeDocument = RecipeDocument,
                AdminPassword = SuperUserPassword,
                AdminUsername = _orchardServices.WorkContext.CurrentSite.SuperUser,
                DatabaseConnectionString = _shellSettings.DataConnectionString,
                DatabaseProvider = _shellSettings.DataProvider,
                DatabaseTablePrefix = _shellSettings.DataTablePrefix,
                SiteName = _orchardServices.WorkContext.CurrentSite.SiteName,
                EnabledFeatures = _featureManager.GetEnabledFeatures().Select(x => x.Id).ToArray()
            };
            return _setupService.Setup(setupContext);
        }

        private string ExecuteRecipe() {
            return _importExportService.Import(RecipeDocument);
        }
    }
}