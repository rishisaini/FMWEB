using System;
using System.Web.Mvc;

namespace AssetCare.Common
{
    public static class Toast
    {
        internal static void ShowError(TempDataDictionary tempData, string message)
        {
            tempData["toaster-notification"] = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                type = Constants.ToastError,
                content = message
            });
        }
        internal static void ShowSuccess(TempDataDictionary tempData, string message)
        {
            tempData["toaster-notification"] = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                type = Constants.ToastSuccess,
                content = message
            });
        }
        internal static void ShowInfo(TempDataDictionary tempData, string message)
        {
            tempData["toaster-notification"] = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                type = Constants.ToastSuccess,
                content = message
            });
        }
        internal static void ShowWarning(TempDataDictionary tempData, string message)
        {
            tempData["toaster-notification"] = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                type = Constants.ToastWarning,
                content = message
            });
        }

    }
}