using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sample.AspNetCore.Models;
using Unleash.Communication.Admin;
using Unleash.Communication.Admin.Dto;

namespace Sample.AspNetCore.Controllers
{
    public class IndexViewModel
    {
        public FeatureToggle[] FeatureToggles { get; set; }
    }

    public class HomeController : Controller
    {
        private IUnleashAdminApiClient AdminApiClient { get; }

        public HomeController(IUnleashAdminApiClient adminApiClient)
        {
            AdminApiClient = adminApiClient;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            await AdminApiClient.Authenticate(@"api@test.com", cancellationToken);

            /*
            var events = await AdminApiClient.Events.GetEvents(cancellationToken);

            var applications = await AdminApiClient.Metrics.GetApplications(cancellationToken);
            var seenApplications = await AdminApiClient.Metrics.GetSeenApplications(cancellationToken);
            var seenToggles = await AdminApiClient.Metrics.GetSeenToggles(cancellationToken);
            var featureTogglesMetrics = await AdminApiClient.Metrics.GetFeatureTogglesMetrics(cancellationToken);
            // AdminApiClient.Metrics.GetApplicationDetails
            // AdminApiClient.Metrics.GetApplicationsImplementingStrategy

            var stateExport = await AdminApiClient.State.GetStateExport(true, true, cancellationToken);

            var strategies = await AdminApiClient.Strategies.GetAllStrategies(cancellationToken);
            var archivedFeatureToggles = await AdminApiClient.FeatureToggles.GetAllArchivedFeatureToggles(cancellationToken);
            */
            // AdminApiClient.Strategies.CreateStrategy
            // AdminApiClient.Strategies.UpdateStrategy

            FeatureToggleResult activeFeatureToggles = await AdminApiClient.FeatureToggles.GetAllActiveFeatureToggles(cancellationToken);
            // TODO:

            var viewModel = new IndexViewModel
            {
                FeatureToggles = activeFeatureToggles.Features
            };
            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
