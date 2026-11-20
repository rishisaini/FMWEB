using AssetCare.Model.ViewModels;
using AssetCare.Repository.Interfaces;
using AssetCare.Service;
using AssetCare.Service.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Results;


namespace AssetCare.Common
{
    public interface IDocumentDownloadHelper
    {
        FileDownloadOutcome GetFileDownloadResult(string token, int agencyId, int userId);
        FileDownloadOutcome GetFileDownloadResult(string token, int userId);
        FileDownloadOutcome GetFileDownloadResultByPath(string token, int userId);
        FileDownloadOutcome GetSitePlan(string encSchoolId, int agencyId, int userId);
        FileDownloadOutcome GetAsbestosManagementPlan(string encSchoolId, int agencyId, int userId);
        FileDownloadOutcome GetAsbestosRegister(string encSchoolIdToken, int agencyId, int userId);
        bool DeleteDocument(int agencyId, string encDocumentId, int userId);
    }

    public class DocumentDownloadHelper : IDocumentDownloadHelper
    {
        private IPropertyService _propertyService;
        private ISupportDocumentRepository _supportDocumentRepository;
        private ICalloutService _calloutService;
        private IDocumentService _documentService;
        private IBaseService _baseService;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public DocumentDownloadHelper(IPropertyService propertyService,
                                        ISupportDocumentRepository supportDocumentRepository,
                                        IDocumentService documentService,
                                        ICalloutService calloutService, IBaseService baseService)
        {
            _propertyService = propertyService;
            _supportDocumentRepository = supportDocumentRepository;
            _calloutService = calloutService;
            _documentService = documentService;
            _baseService = baseService;
        }

        public FileDownloadOutcome GetFileDownloadResult(string token, int agencyId, int userId)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return FileDownloadOutcome.Fail(FileErrorType.InvalidPath, "Invalid or missing file parameter.");
            }

            var result = GetFileResponse(token, agencyId, userId);

            if (!result.Success)
            {
                return FileDownloadOutcome.Fail(result.ErrorType, result.ErrorMessage);
            }

            return FileDownloadOutcome.Ok(result.Response);
        }

        public FileDownloadOutcome GetFileDownloadResult(string token, int userId)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return FileDownloadOutcome.Fail(FileErrorType.InvalidPath, "Invalid or missing file parameter.");
            }

            var result = GetFileResponse(token, userId);

            if (!result.Success)
            {
                return FileDownloadOutcome.Fail(result.ErrorType, result.ErrorMessage);
            }

            return FileDownloadOutcome.Ok(result.Response);
        }

        public FileDownloadOutcome GetFileDownloadResultByPath(string encryptedPath, int userId)
        {
            if (string.IsNullOrWhiteSpace(encryptedPath))
            {
                return FileDownloadOutcome.Fail(FileErrorType.InvalidPath, "Invalid or missing file parameter.");
            }

            var result = GetFileResponseByPath(encryptedPath, userId);

            if (!result.Success)
            {
                return FileDownloadOutcome.Fail(result.ErrorType, result.ErrorMessage);
            }

            return FileDownloadOutcome.Ok(result.Response);
        }

        public FileDownloadOutcome GetSitePlan(string encSchoolIdToken, int agencyId, int userId)
        {
            string schoolIdStr;
            try
            {
                schoolIdStr = AesIdProtector.Decrypt(encSchoolIdToken).ToString();
            }
            catch (Exception)
            {
                return FileDownloadOutcome.Fail(FileErrorType.InvalidPath, "Invalid school id.");
            }

            if (schoolIdStr.Length == 3)
            {
                schoolIdStr = $"0{schoolIdStr}";
            }

            string[] sitePlans;
            try
            {
                sitePlans = Directory.GetFiles(HostingEnvironment.MapPath("~/Content/DETSchoolPlans/All"),
                    $"*_{schoolIdStr}.pdf");
            }
            catch (Exception e)
            {
                return FileDownloadOutcome.Fail(FileErrorType.Unknown, e.Message + e.InnerException);
            }

            if (sitePlans.Length == 0)
            {
                return FileDownloadOutcome.Fail(FileErrorType.NotFound, "Site plan not found.");
            }

            var sitePlanFileName = Path.GetFileName(sitePlans[0]);
            var relativePath = $"/Content/DETSchoolPlans/All/{sitePlanFileName}";


            if (!File.Exists(HostingEnvironment.MapPath($"~{relativePath}")))
            {
                return FileDownloadOutcome.Fail(FileErrorType.NotFound, "Site plan not found.");
            }

            string physicalPath = HostingEnvironment.MapPath("~/" + relativePath);


            var result = GetSitePlanResponse(physicalPath, userId, agencyId, AesIdProtector.Decrypt(encSchoolIdToken));

            if (!result.Success)
            {
                return FileDownloadOutcome.Fail(result.ErrorType, result.ErrorMessage);
            }

            string userAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"];
            bool isMobile = new SharedFunctions().IsMobileBrowsers(userAgent);
            result.Response.Headers.Add("X-Is-Mobile-Browser", isMobile.ToString());

            return FileDownloadOutcome.Ok(result.Response);
        }

        public FileDownloadOutcome GetAsbestosManagementPlan(string encSchoolIdToken, int agencyId, int userId)
        {
            string schoolIdStr;
            try
            {
                schoolIdStr = AesIdProtector.Decrypt(encSchoolIdToken).ToString();
            }
            catch (Exception)
            {
                return FileDownloadOutcome.Fail(FileErrorType.InvalidPath, "Invalid school id.");
            }

            if (schoolIdStr.Length == 3)
            {
                schoolIdStr = $"0{schoolIdStr}";
            }

            string[] pFiles;
            try
            {
                pFiles = Directory.GetFiles(HostingEnvironment.MapPath($"~/Content/DETAsbestosManagementPlans/{schoolIdStr[0]}/{schoolIdStr}/asbestos-register"), $"*site specific management plan*.pdf");
            }
            catch (Exception e)
            {
                return FileDownloadOutcome.Fail(FileErrorType.Unknown, e.Message);
            }

            if (pFiles.Length == 0)
            {
                return FileDownloadOutcome.Fail(FileErrorType.NotFound, "Site plan not found.");
            }

            var filename = Path.GetFileName(pFiles[0]);
            var asbestosRegisterPDFPath = $"/Content/DETAsbestosManagementPlans/{schoolIdStr[0]}/{schoolIdStr}/asbestos-register/{filename}";



            if (!File.Exists(HostingEnvironment.MapPath($"~{asbestosRegisterPDFPath}")))
            {
                return FileDownloadOutcome.Fail(FileErrorType.NotFound, "Site plan not found.");
            }

            string physicalPath = HostingEnvironment.MapPath("~/" + asbestosRegisterPDFPath);

            var result = GetSitePlanResponse(physicalPath, userId, agencyId, AesIdProtector.Decrypt(encSchoolIdToken));

            if (!result.Success)
            {
                return FileDownloadOutcome.Fail(result.ErrorType, result.ErrorMessage);
            }

            string userAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"];
            bool isMobile = new SharedFunctions().IsMobileBrowsers(userAgent);
            result.Response.Headers.Add("X-Is-Mobile-Browser", isMobile.ToString());

            return FileDownloadOutcome.Ok(result.Response);
        }

        public FileDownloadOutcome GetAsbestosRegister(string encSchoolIdToken, int agencyId, int userId)
        {
            string schoolIdStr;

            try
            {
                schoolIdStr = AesIdProtector.Decrypt(encSchoolIdToken).ToString();
            }
            catch (Exception)
            {
                return FileDownloadOutcome.Fail(
                    FileErrorType.InvalidPath,
                    "Invalid school id.");
            }

            if (schoolIdStr.Length == 3)
            {
                schoolIdStr = $"0{schoolIdStr}";
            }

            string[] pFiles;

            try
            {
                string basePath = HostingEnvironment.MapPath(
                    "~/Content/DETAsbestosRegisters/All");

                pFiles = Directory.GetFiles(
                    basePath,
                    $"{schoolIdStr}_*.pdf");
            }
            catch (Exception e)
            {
                return FileDownloadOutcome.Fail(
                    FileErrorType.Unknown,
                    e.Message);
            }

            if (pFiles.Length == 0)
            {
                return FileDownloadOutcome.Fail(
                    FileErrorType.NotFound,
                    "Asbestos register not found.");
            }

            string filename = Path.GetFileName(pFiles[0]);

            string asbestosRegisterPDFPath =
                $"/Content/DETAsbestosRegisters/All/{filename}";

            string physicalPath = HostingEnvironment.MapPath(
                "~" + asbestosRegisterPDFPath);

            if (string.IsNullOrEmpty(physicalPath) || !File.Exists(physicalPath))
            {
                return FileDownloadOutcome.Fail(
                    FileErrorType.NotFound,
                    "Asbestos register not found.");
            }

            var result = GetSitePlanResponse(
                physicalPath,
                userId,
                agencyId,
                AesIdProtector.Decrypt(encSchoolIdToken));

            if (!result.Success)
            {
                return FileDownloadOutcome.Fail(
                    result.ErrorType,
                    result.ErrorMessage);
            }

            string userAgent =
                HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"];

            bool isMobile =
                new SharedFunctions().IsMobileBrowsers(userAgent);

            result.Response.Headers.Add(
                "X-Is-Mobile-Browser",
                isMobile.ToString());

            return FileDownloadOutcome.Ok(result.Response);
        }
        public bool DeleteDocument(int agencyId, string encDocumentId, int userId)
        {
            int schoolId = 0;
            int documentId = 0;
            try
            {
                documentId = AesIdProtector.Decrypt(encDocumentId);
                var document = _supportDocumentRepository.GetSupportDocument(documentId);

                if (document.Property != null)
                {
                    schoolId = document.Property.SchoolId.Value;
                    if (document.AgencyID == agencyId &&
                        _propertyService.UserHasAccessToProperty(userId, agencyId, schoolId))
                    {
                        _calloutService.DeleteDocument(documentId);
                        return true;
                    }
                }
                else if (document.Property == null &&
                   _baseService.GetCurrentUser().UserAgencyIds.Count() > 0 && _baseService.GetCurrentUser().UserAgencyIds.Contains(document.AgencyID))
                {
                    _calloutService.DeleteDocument(documentId);
                    return true;
                }
            }
            catch (Exception e)
            {
                //error
            }

            return false;
        }

        private FileResult GetSitePlanResponse(string physicalPath, int userId, int agencyId, int schoolId)
        {
            string rootPath = HostingEnvironment.MapPath("~/");
            string fullRoot = Path.GetFullPath(rootPath);
            string fullTarget = Path.GetFullPath(physicalPath);
            string folderSharePath = ConfigurationManager.AppSettings["FolderShareUNCPath"];

            if (!HttpContext.Current.Request.IsLocal)
            {

                if (!fullTarget.StartsWith(folderSharePath, StringComparison.OrdinalIgnoreCase))
                {
                    return new FileResult
                    {
                        Success = false,
                        ErrorType = FileErrorType.InvalidPath,
                        ErrorMessage = "Resolved path is outside the allowed directory."
                    };
                }
            }


            if (!_propertyService.UserHasAccessToProperty(userId, agencyId, schoolId))
            {
                return new FileResult
                {
                    Success = false,
                    ErrorType = FileErrorType.AccessDenied,
                    ErrorMessage = "You are not allowed to access that file."
                };
            }

            if (!File.Exists(physicalPath))
            {
                return new FileResult
                {
                    Success = false,
                    ErrorType = FileErrorType.NotFound,
                    ErrorMessage = "File not found."
                };
            }

            var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream)
            };

            string contentType = MimeMapping.GetMimeMapping(physicalPath);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline")
            {
                FileName = Path.GetFileName(physicalPath)
            };

            return new FileResult
            {
                Success = true,
                ErrorType = FileErrorType.None,
                Response = response
            };
        }

        /// <summary>
        /// Decrypts the id, resolves the physical path, validates it's within
        /// the allowed root, and builds an inline HttpResponseMessage for the file.
        /// </summary>
        /// <summary>
        /// Decrypts the id, resolves the physical path, validates it's within
        /// the allowed root, and builds an inline HttpResponseMessage for the file.
        /// </summary>
        private Model.ViewModels.FileResult GetFileResponse(string token, int agencyId, int userId)
        {
            string fileUrl;
            int documentId = 0;

            try
            {
                documentId = AesIdProtector.Decrypt(token);
            }
            catch (Exception)
            {
                return new Model.ViewModels.FileResult
                {
                    Success = false,
                    ErrorType = FileErrorType.InvalidPath,
                    ErrorMessage = "Malformed file URL parameter."
                };
            }

            try
            {
                var doc = _supportDocumentRepository.GetSupportDocument(documentId);
                fileUrl = doc.AttachmentLink ?? string.Empty;

                // Compare against the actual resolved SupportDocuments root, not the
                // site root — SupportDocuments is mapped to a UNC file share in some
                // environments, so it won't be a subpath of "~/".
                string documentsRoot = Path.GetFullPath(HostingEnvironment.MapPath("~/Content/SupportDocuments/"));

                // Candidate 1: original resolution — handles the "~/..." tilde format
                // used by Reactive/OnDemand, e.g. "~/Content/SupportDocuments//8061000/..."
                string normalized = StringExtensions.RemoveLeadingTilde(fileUrl).Replace("//", "/").TrimStart('/');
                string candidate1 = HostingEnvironment.MapPath("~/" + normalized);

                // Candidate 2: strip a leading app-virtual-path segment — handles the
                // "/FMClean/Content/..." format used by Proactive uploads
                string candidate2 = null;
                var appPath = (System.Web.HttpRuntime.AppDomainAppVirtualPath ?? "/").TrimEnd('/');
                if (!string.IsNullOrEmpty(appPath))
                {
                    var link = fileUrl.Trim().Replace("//", "/");
                    if (link.StartsWith(appPath + "/", StringComparison.OrdinalIgnoreCase))
                    {
                        var stripped = link.Substring(appPath.Length + 1);
                        candidate2 = HostingEnvironment.MapPath("~/" + stripped);
                    }
                }

                string fullTarget = Path.GetFullPath(candidate1);
                if (!File.Exists(fullTarget) && candidate2 != null)
                {
                    var altFull = Path.GetFullPath(candidate2);
                    if (File.Exists(altFull))
                    {
                        fullTarget = altFull;
                    }
                }


                // Guard against path traversal: resolved path must stay under the
                // SupportDocuments root, regardless of which candidate was used.
                if (!fullTarget.StartsWith(documentsRoot, StringComparison.OrdinalIgnoreCase))
                {
                    return new FileResult
                    {
                        Success = false,
                        ErrorType = FileErrorType.InvalidPath,
                        ErrorMessage = "Resolved path is outside the allowed directory."
                    };
                }

                if (doc.AgencyID != agencyId ||
                    (_baseService.GetCurrentUser().UserAgencyIds.Count() > 0 && !_baseService.GetCurrentUser().UserAgencyIds.Contains(doc.AgencyID)))
                {
                    return new FileResult
                    {
                        Success = false,
                        ErrorType = FileErrorType.AccessDenied,
                        ErrorMessage = "You are not allowed to access that region"
                    };
                }

                //contractor register will have no property/schoolid associated with document
                if (doc.Property != null && doc.Property.SchoolId.HasValue && !_propertyService.UserHasAccessToProperty(userId, agencyId, doc.Property.SchoolId.Value))
                {
                    return new FileResult
                    {
                        Success = false,
                        ErrorType = FileErrorType.AccessDenied,
                        ErrorMessage = "You are not allowed to access that file."
                    };
                }

                if (!File.Exists(fullTarget))
                {
                    return new FileResult
                    {
                        Success = false,
                        ErrorType = FileErrorType.NotFound,
                        ErrorMessage = "File not found."
                    };
                }

                var stream = new FileStream(fullTarget, FileMode.Open, FileAccess.Read);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(stream)
                };

                string contentType = MimeMapping.GetMimeMapping(fullTarget);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline")
                {
                    FileName = Path.GetFileName(fullTarget)
                };

                return new FileResult
                {
                    Success = true,
                    ErrorType = FileErrorType.None,
                    Response = response
                };
            }
            catch (UnauthorizedAccessException)
            {
                return new FileResult
                {
                    Success = false,
                    ErrorType = FileErrorType.AccessDenied,
                    ErrorMessage = "Permission denied accessing file."
                };
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return new FileResult
                {
                    Success = false,
                    ErrorType = FileErrorType.Unknown,
                    ErrorMessage = "Unknown error"
                };
            }
        }

        /// <summary>
        /// Decrypts the id, resolves the physical path, validates it's within
        /// the allowed root, and builds an inline HttpResponseMessage for the file.
        /// </summary>
        /// <summary>
        /// Decrypts the id, resolves the physical path, validates it's within
        /// the allowed root, and builds an inline HttpResponseMessage for the file.
        /// </summary>
        private Model.ViewModels.FileResult GetFileResponse(string token, int userId)
        {
            string fileUrl;
            int documentId = 0;

            try
            {
                documentId = AesIdProtector.Decrypt(token);
            }
            catch (Exception)
            {
                return new Model.ViewModels.FileResult
                {
                    Success = false,
                    ErrorType = FileErrorType.InvalidPath,
                    ErrorMessage = "Malformed file URL parameter."
                };
            }

            try
            {
                var doc = _documentService.GetDocument(documentId);
                fileUrl = doc.Url ?? string.Empty;

                // Compare against the actual resolved SupportDocuments root, not the
                // site root — SupportDocuments is mapped to a UNC file share in some
                // environments, so it won't be a subpath of "~/".
                string documentsRoot = Path.GetFullPath(HostingEnvironment.MapPath(DocumentType.BaseUrl));

                // Candidate 1: original resolution — handles the "~/..." tilde format
                // used by Reactive/OnDemand, e.g. "~/Content/SupportDocuments//8061000/..."
                string normalized = StringExtensions.RemoveLeadingTilde(fileUrl).Replace("//", "/").TrimStart('/');
                string candidate1 = HostingEnvironment.MapPath("~/" + normalized);

                //// Candidate 2: strip a leading app-virtual-path segment — handles the
                //// "/FMClean/Content/..." format used by Proactive uploads
                //string candidate2 = null;
                //var appPath = (System.Web.HttpRuntime.AppDomainAppVirtualPath ?? "/").TrimEnd('/');
                //if (!string.IsNullOrEmpty(appPath))
                //{
                //    var link = fileUrl.Trim().Replace("//", "/");
                //    if (link.StartsWith(appPath + "/", StringComparison.OrdinalIgnoreCase))
                //    {
                //        var stripped = link.Substring(appPath.Length + 1);
                //        candidate2 = HostingEnvironment.MapPath("~/" + stripped);
                //    }
                //}

                string fullTarget = Path.GetFullPath(candidate1);
                //if (!File.Exists(fullTarget) && candidate2 != null)
                //{
                //    var altFull = Path.GetFullPath(candidate2);
                //    if (File.Exists(altFull))
                //    {
                //        fullTarget = altFull;
                //    }
                //}


                // Guard against path traversal: resolved path must stay under the
                // SupportDocuments root, regardless of which candidate was used.
                if (!fullTarget.StartsWith(documentsRoot, StringComparison.OrdinalIgnoreCase))
                {
                    return new FileResult
                    {
                        Success = false,
                        ErrorType = FileErrorType.InvalidPath,
                        ErrorMessage = "Resolved path is outside the allowed directory."
                    };
                }

                if (!File.Exists(fullTarget))
                {
                    return new FileResult
                    {
                        Success = false,
                        ErrorType = FileErrorType.NotFound,
                        ErrorMessage = "File not found."
                    };
                }

                var stream = new FileStream(fullTarget, FileMode.Open, FileAccess.Read);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(stream)
                };

                string contentType = MimeMapping.GetMimeMapping(fullTarget);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline")
                {
                    FileName = Path.GetFileName(fullTarget)
                };

                return new FileResult
                {
                    Success = true,
                    ErrorType = FileErrorType.None,
                    Response = response
                };
            }
            catch (UnauthorizedAccessException)
            {
                return new FileResult
                {
                    Success = false,
                    ErrorType = FileErrorType.AccessDenied,
                    ErrorMessage = "Permission denied accessing file."
                };
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return new FileResult
                {
                    Success = false,
                    ErrorType = FileErrorType.Unknown,
                    ErrorMessage = "Unknown error"
                };
            }
        }
        private Model.ViewModels.FileResult GetFileResponseByPath(string encryptedPath, int userId)
        {
            string fileUrl;
            try
            {
                fileUrl = AesIdProtector.DecryptText(encryptedPath);

                // Compare against the actual resolved SupportDocuments root, not the
                // site root — SupportDocuments is mapped to a UNC file share in some
                // environments, so it won't be a subpath of "~/".
                string documentsRoot = Path.GetFullPath(HostingEnvironment.MapPath(DocumentType.ContentBaseUrl));

                // Candidate 1: original resolution — handles the "~/..." tilde format
                // used by Reactive/OnDemand, e.g. "~/Content/SupportDocuments//8061000/..."
                string normalized = StringExtensions.RemoveLeadingTilde(fileUrl).Replace("//", "/").TrimStart('/');
                string candidate1 = HostingEnvironment.MapPath("~/" + normalized);

                //// Candidate 2: strip a leading app-virtual-path segment — handles the
                //// "/FMClean/Content/..." format used by Proactive uploads
                //string candidate2 = null;
                //var appPath = (System.Web.HttpRuntime.AppDomainAppVirtualPath ?? "/").TrimEnd('/');
                //if (!string.IsNullOrEmpty(appPath))
                //{
                //    var link = fileUrl.Trim().Replace("//", "/");
                //    if (link.StartsWith(appPath + "/", StringComparison.OrdinalIgnoreCase))
                //    {
                //        var stripped = link.Substring(appPath.Length + 1);
                //        candidate2 = HostingEnvironment.MapPath("~/" + stripped);
                //    }
                //}

                string fullTarget = Path.GetFullPath(candidate1);
                //if (!File.Exists(fullTarget) && candidate2 != null)
                //{
                //    var altFull = Path.GetFullPath(candidate2);
                //    if (File.Exists(altFull))
                //    {
                //        fullTarget = altFull;
                //    }
                //}


                // Guard against path traversal: resolved path must stay under the
                // SupportDocuments root, regardless of which candidate was used.
                if (!fullTarget.StartsWith(documentsRoot, StringComparison.OrdinalIgnoreCase))
                {
                    return new FileResult
                    {
                        Success = false,
                        ErrorType = FileErrorType.InvalidPath,
                        ErrorMessage = "Resolved path is outside the allowed directory."
                    };
                }

                if (!File.Exists(fullTarget))
                {
                    return new FileResult
                    {
                        Success = false,
                        ErrorType = FileErrorType.NotFound,
                        ErrorMessage = "File not found."
                    };
                }

                var stream = new FileStream(fullTarget, FileMode.Open, FileAccess.Read);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(stream)
                };

                string contentType = MimeMapping.GetMimeMapping(fullTarget);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline")
                {
                    FileName = Path.GetFileName(fullTarget)
                };

                return new FileResult
                {
                    Success = true,
                    ErrorType = FileErrorType.None,
                    Response = response
                };
            }
            catch (UnauthorizedAccessException)
            {
                return new FileResult
                {
                    Success = false,
                    ErrorType = FileErrorType.AccessDenied,
                    ErrorMessage = "Permission denied accessing file."
                };
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return new FileResult
                {
                    Success = false,
                    ErrorType = FileErrorType.Unknown,
                    ErrorMessage = "Unknown error"
                };
            }
        }
    }
}