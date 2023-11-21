using Intalio.CTS.Custom.DAL.Managers;
using Intalio.CTS.Custom.DAL;
using Microsoft.Extensions.Logging;
using Intalio.CTS.Custom.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;

namespace Intalio.CTS.Custom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : BaseController
    {
        private DapperContext _dapperContext;
        private DashboardManager _dashboardManager;

        public DashboardController(ILogger<DashboardController> logger, DapperContext dapperContext, DashboardManager dashboardManager) : base(logger)
        {
            this._dapperContext = dapperContext;
            this._dashboardManager = dashboardManager;
        }

        [HttpPost("AverageCompletion")]
        public async Task<IActionResult> AverageCompletion(string toStructureId = null, string yearFromTo = null)
        {
            try
            {
                var retVal = await _dashboardManager.GetAverageCompletion(UserContext.Local.StructureId, UserContext.Local.Id, UserContext.Local.IsStructureReceiver, UserContext.Local.PrivacyLevel, toStructureId, yearFromTo);
                return Ok(retVal);
            }
            catch (Exception ex)
            {
                CtsExecutionContext.Local.ActionResult = CtsActionResult.Failure;
                return HttpAjaxError(ex);
            }
            finally
            {
                await AuditManager.LogAction("SearchController.GetAverageCompletion",
                    $"query: fromStructureId: {UserContext.Local.StructureId}, fromUserId: {UserContext.Local.Id}, toStructureId: {toStructureId}, yearsSpan: {yearFromTo}");
            }
        }

        [HttpPost("AverageDelay")]
        public async Task<IActionResult> AverageDelay(string toStructureId = null, string yearFromTo = null)
        {
            try
            {
                var retVal = await _dashboardManager.GetAverageDelay(UserContext.Local.StructureId, UserContext.Local.Id, UserContext.Local.IsStructureReceiver, UserContext.Local.PrivacyLevel, toStructureId, yearFromTo);
                return Ok(retVal);
            }
            catch (Exception ex)
            {
                CtsExecutionContext.Local.ActionResult = CtsActionResult.Failure;
                return HttpAjaxError(ex);
            }
            finally
            {
                await AuditManager.LogAction("SearchController.GetAverageDelay",
                    $"query: fromStructureId: {UserContext.Local.StructureId}, fromUserId: {UserContext.Local.Id}, toStructureId: {toStructureId}, yearsSpan: {yearFromTo}");
            }
        }

        [HttpPost("DashboardCounts")]
        public async Task<IActionResult> DashboardCounts(string toStructureId = null, string yearFromTo = null)
        {
            try
            {
                var retVal = await _dashboardManager.GetDashboardCounts(UserContext.Local.StructureId, UserContext.Local.Id, UserContext.Local.IsStructureReceiver, UserContext.Local.PrivacyLevel, toStructureId, yearFromTo);
                return Ok(retVal);
            }
            catch (Exception ex)
            {
                CtsExecutionContext.Local.ActionResult = CtsActionResult.Failure;
                return HttpAjaxError(ex);
            }
            finally
            {
                await AuditManager.LogAction("SearchController.GetOpenDelayedCounts",
                    $"query: fromStructureId: {UserContext.Local.StructureId}, fromUserId: {UserContext.Local.Id}, toStructureId: {toStructureId}, yearsSpan: {yearFromTo}");
            }
        }

        [HttpPost("ReceivedVsClosed")]
        public async Task<IActionResult> ReceivedVsClosed(string toStructureId = null, string yearFromTo = null)
        {
            try
            {
                var retVal = await _dashboardManager.GetReceivedVsClosedCounts(UserContext.Local.StructureId, UserContext.Local.Id, UserContext.Local.IsStructureReceiver, UserContext.Local.PrivacyLevel, toStructureId, yearFromTo);
                return Ok(retVal);
            }
            catch (Exception ex)
            {
                CtsExecutionContext.Local.ActionResult = CtsActionResult.Failure;
                return HttpAjaxError(ex);
            }
            finally
            {
                await AuditManager.LogAction("SearchController.GetOpenDelayedCounts",
                    $"query: fromStructureId: {UserContext.Local.StructureId}, fromUserId: {UserContext.Local.Id}, toStructureId: {toStructureId}");
            }
        }

        [HttpPost("ReceivedByCategories")]
        public async Task<IActionResult> ReceivedByCategories(string toStructureId = null, string yearFromTo = null)
        {
            try
            {
                var retVal = await _dashboardManager.GetReceivedCategorized(UserContext.Local.StructureId, UserContext.Local.Id, UserContext.Local.IsStructureReceiver, UserContext.Local.PrivacyLevel, toStructureId, yearFromTo);
                return Ok(retVal);
            }
            catch (Exception ex)
            {
                CtsExecutionContext.Local.ActionResult = CtsActionResult.Failure;
                return HttpAjaxError(ex);
            }
            finally
            {
                await AuditManager.LogAction("SearchController.GetOpenDelayedCounts",
                    $"query: fromStructureId: {UserContext.Local.StructureId}, fromUserId: {UserContext.Local.Id}, toStructureId: {toStructureId}");
            }
        }

        [HttpPost("ClosedByCategories")]
        public async Task<IActionResult> ClosedByCategories(string toStructureId = null, string yearFromTo = null)
        {
            try
            {
                var retVal = await _dashboardManager.GetClosedCategorized(UserContext.Local.StructureId, UserContext.Local.Id, UserContext.Local.IsStructureReceiver, UserContext.Local.PrivacyLevel, toStructureId, yearFromTo);
                return Ok(retVal);
            }
            catch (Exception ex)
            {
                CtsExecutionContext.Local.ActionResult = CtsActionResult.Failure;
                return HttpAjaxError(ex);
            }
            finally
            {
                await AuditManager.LogAction("SearchController.GetOpenDelayedCounts",
                    $"query: fromStructureId: {UserContext.Local.StructureId}, fromUserId: {UserContext.Local.Id}, toStructureId: {toStructureId}");
            }
        }

        [HttpPost("OverDueByCategories")]
        public async Task<IActionResult> OverDueByCategories(string toStructureId = null, string yearFromTo = null)
        {
            try
            {
                var retVal = await _dashboardManager.GetOverdueCategorized(UserContext.Local.StructureId, UserContext.Local.Id, UserContext.Local.IsStructureReceiver, UserContext.Local.PrivacyLevel, toStructureId, yearFromTo);
                return Ok(retVal);
            }
            catch (Exception ex)
            {
                CtsExecutionContext.Local.ActionResult = CtsActionResult.Failure;
                return HttpAjaxError(ex);
            }
            finally
            {
                await AuditManager.LogAction("SearchController.GetOpenDelayedCounts",
                    $"query: fromStructureId: {UserContext.Local.StructureId}, fromUserId: {UserContext.Local.Id}, toStructureId: {toStructureId}");
            }
        }
    }
}
