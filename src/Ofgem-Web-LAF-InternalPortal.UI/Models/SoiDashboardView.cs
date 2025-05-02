namespace Ofgem_Web_LAF_InternalPortal.Models;

public class DashboardView
{
    public List<Models.StatementOfIntentV2> StatementOfIntents { get; set; } = [];

    public int CurrentPage { get; set; } = 0;
    public int PageCount { get; set; } = 0;
    public int PageSize { get; set; } = 0;
    public int RowCount { get; set; } = 0;
    public int FirstRowOnPage { get; set; } = 0;
    public int LastRowOnPage { get; set; } = 0;
}