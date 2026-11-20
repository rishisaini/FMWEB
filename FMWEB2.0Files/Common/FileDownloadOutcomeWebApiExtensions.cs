using AssetCare.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace AssetCare.Common
{
    public static class FileDownloadOutcomeWebApiExtensions
    {
        public static IHttpActionResult ToHttpActionResult(this FileDownloadOutcome outcome, ApiController controller)
        {
            if (!outcome.Success)
            {
                switch (outcome.ErrorType)
                {
                    case FileErrorType.NotFound:
                        return new NotFoundResult(controller);
                    case FileErrorType.InvalidPath:
                        return new BadRequestErrorMessageResult(outcome.ErrorMessage, controller);
                    case FileErrorType.AccessDenied:
                    default:
                        return new ExceptionResult(new Exception(outcome.ErrorMessage), controller);
                }
            }

            return new ResponseMessageResult(outcome.Response);
        }
    }
}