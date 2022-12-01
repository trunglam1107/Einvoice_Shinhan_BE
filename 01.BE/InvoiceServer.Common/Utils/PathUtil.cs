using System;

namespace InvoiceServer.Common
{
    public static class PathUtil
    {
        public static string UrlCombine(params string[] paths)
        {
            string urlResult = "";
            for (int i = 0; i < paths.Length; i++)
            {
                string url1 = urlResult.TrimEnd('/');
                string url2 = paths[i].TrimStart('/');
                if (string.IsNullOrEmpty(url1))
                {
                    urlResult = url2;
                }
                else
                {
                    urlResult = string.Format("{0}/{1}", url1, url2);
                }
            }
            return urlResult;
        }

        public static string FolderTree(DateTime? dateRelease = null)
        {
            var now = DateTime.Now;
            string folderTree = now.Year.ToString() + "\\" + now.ToString("MM") + "\\" + now.ToString("dd");
            if (dateRelease != null)
            {
                folderTree = dateRelease.Value.Year.ToString() + "\\" + dateRelease.Value.ToString("MM") + "\\" + dateRelease.Value.ToString("dd");
            }
            return folderTree;
        }
    }
}
