namespace Ofgem_Web_LAF_InternalPortal;

public static class LogEvents
{
    public const int UploadFile = 1000;
    public const int DeleteFile = 1001;
    public const int CreateBlob = 1002;
    public const int GetBlob = 1003;

    public const int UploadDocument = 2000;
    public const int NewDocument = 2001;
    public const int GetDocument = 2002;
    public const int DeleteDocument = 2003;
    public const int UpdateDocument = 2004;
    public const int GetAllDocuments = 2005;

    public const int ValidateFile = 3500;
    public const int RedactionService = 3501;

    public const int Announcements = 4000;
    public const int EditAnnouncement = 4001;
    public const int Admin = 5000;

    public const int Profiles = 6000;

    public const int ExternalUsers = 7000;
    public const int GetSoi = 80000;
    public const int Health = 9000;
    public const int HealthFull = 9001;
    public const int GetLocalAuthorities = 90013;

    public const int GetUploads = 10000;

}