namespace SitecoreSeoAnalyzerApp.Models
{
    /// <summary>
    /// Default error view
    /// </summary>
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
