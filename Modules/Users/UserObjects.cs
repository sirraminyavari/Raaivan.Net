﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;


namespace RaaiVan.Modules.Users
{
    public enum VisitItemTypes
    {
        Knowledge,
        Node,
        User,
        Question
    }

    public enum EmploymentType
    {
        NotSet,
        Permanent, //Rasmi
        Temporary, //Gharardadi
        Contract, //Peymani
        Other
    }

    public enum PhoneType
    {
        NotSet,
        Mobile,
        Home,
        Office,
        Fax,
        Other
    }

    public enum GraduateDegree
    {
        First,
        Second,
        Third,
        Other,
        None
    }

    public enum EducationalLevel
    {
        Diploma,
        PostDiploma,
        Bachelor,
        Master,
        Doctor,
        None
    }

    public enum LanguageLevel
    {
        Beginner,
        Intermediate,
        Advanced,
        None
    }

    public static class UserUtilities
    {
        private static Dictionary<Guid, User> _SystemUsers = new Dictionary<Guid, User>();
        public static User SystemUser(Guid applicationId)
        {
            try
            {
                if (_SystemUsers.ContainsKey(applicationId)) return _SystemUsers[applicationId];
                else
                {
                    User su = UsersController.get_system_user(applicationId);
                    if (su != null) _SystemUsers[applicationId] = su;
                    return su;
                }
            }
            catch { return null; }
        }

        public static bool check_user_existence_in_active_directory(Guid? applicationId, 
            string domainName, string username, ref string errorMessage)
        {
            try
            {
                if (RaaiVanSettings.Users.IgnoreActiveDirectoryUserCheck(applicationId)) return true;

                PrincipalContext ctx = new PrincipalContext(ContextType.Domain, /* "LDAP://" +  */ domainName);
                return UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, username) != null;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, "ActiveDir_UserExistanceCheck", ex, ModuleIdentifier.RV);
                errorMessage = "CheckingUserExistanceInActiveDirectoryFailed";
                return false;
            }
        }

        public static bool is_active_directory_authenticated(Guid? applicationId, 
            string domainName, string username, string password, ref string errorMessage)
        {
            try
            {
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain, /* "LDAP://" +  */ domainName);
                return ctx.ValidateCredentials(username, password);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, "ActiveDir_UserExistanceCheck", ex, ModuleIdentifier.RV);
                errorMessage = "ValidatingUserInActiveDirectoryFailed";
                return false;
            }

            /*
            try
            {
                DirectoryEntry entry = new DirectoryEntry("LDAP://" + domainName, domainName + @"\" + username, password);
                DirectorySearcher searcher = new DirectorySearcher(entry);
                searcher.Filter = "(SamAccountName=" + username + ")";
                searcher.PropertiesToLoad.Add("cn");

                return searcher.FindOne() != null;
            }
            catch (Exception ex) { return false; }
            */
        }

        public static List<EmploymentType> get_employment_types()
        {
            List<EmploymentType> types = new List<EmploymentType>();

            Array arr = Enum.GetValues(typeof(EmploymentType));
            for (int i = 0, lnt = arr.Length; i < lnt; ++i)
                if ((EmploymentType)arr.GetValue(i) != EmploymentType.NotSet) types.Add((EmploymentType)arr.GetValue(i));

            return types;
        }

        public static string generate_password_salt()
        {
            var buf = new byte[16];
            (new System.Security.Cryptography.RNGCryptoServiceProvider()).GetBytes(buf);
            return Convert.ToBase64String(buf);
        }

        public static string encode_password(string password, string salt)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(password);
            byte[] src = Convert.FromBase64String(salt);
            byte[] dst = new byte[src.Length + bytes.Length];
            Buffer.BlockCopy(src, 0, dst, 0, src.Length);
            Buffer.BlockCopy(bytes, 0, dst, src.Length, bytes.Length);
            System.Security.Cryptography.HashAlgorithm algorithm = System.Security.Cryptography.HashAlgorithm.Create("SHA1");
            byte[] inArray = algorithm.ComputeHash(dst);
            return Convert.ToBase64String(inArray);
        }

        public static string generate_password(int length = 12)
        {
            string refStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_@^*$!";

            Random rnd = new Random((int)DateTime.Now.Ticks);

            string password = string.Empty;
            for (int i = 0; i < length; ++i)
                password += refStr[rnd.Next(0, refStr.Length - 1)];

            return password;
        }
        
        public static bool check_password_policy(Guid? applicationId, string password, string oldPassword, ref string errorMessage)
        {
            bool meetsLength = password.Length >= Math.Max(1, RaaiVanSettings.Users.PasswordPolicy.MinLength(applicationId));
            bool meetsNewCharacters = oldPassword == null || PublicMethods.diff(password, oldPassword).Length >= 
                RaaiVanSettings.Users.PasswordPolicy.NewCharacters(applicationId);
            bool meetsUpperLower = !RaaiVanSettings.Users.PasswordPolicy.UpperLower(applicationId) || (
                (new Regex("[A-Z]").IsMatch(password)) && (new Regex("[a-z]").IsMatch(password)));
            bool meetsNonAlphabetic = !RaaiVanSettings.Users.PasswordPolicy.NonAlphabetic(applicationId) ||
                !(new Regex("^[a-zA-Z]+$").IsMatch(password));
            bool meetsNumber = !RaaiVanSettings.Users.PasswordPolicy.Number(applicationId) ||
                (new Regex("[0-9]").IsMatch(password));
            bool meetsNonAlphaNumeric = !RaaiVanSettings.Users.PasswordPolicy.NonAlphaNumeric(applicationId) ||
                !(new Regex("^[a-zA-Z0-9]+$").IsMatch(password));

            List<string> notMeet = new List<string>();

            if (!meetsLength) notMeet.Add("MinLength_" + RaaiVanSettings.Users.PasswordPolicy.MinLength(applicationId).ToString());
            if (!meetsNewCharacters) notMeet.Add("NewCharacters_" + RaaiVanSettings.Users.PasswordPolicy.NewCharacters(applicationId).ToString());
            if (!meetsUpperLower) notMeet.Add("UpperLower");
            if (!meetsNonAlphabetic) notMeet.Add("NonAlphabetic");
            if (!meetsNumber) notMeet.Add("Number");
            if (!meetsNonAlphaNumeric) notMeet.Add("NonAlphaNumeric");

            if (notMeet.Count > 0)
            {
                errorMessage = "{\"ErrorText\":\"" + Messages.PasswordPolicyDidntMeet.ToString() + "\"" +
                    ",\"NotMeetItems\":[" + string.Join(",", notMeet.Select(u => "\"" + u + "\"").ToList()) + "]}";
                return false;
            }

            return true;
        }

        public static bool check_password_policy(Guid? applicationId, string password, string oldPassword)
        {
            string str = string.Empty;
            return check_password_policy(applicationId, password, oldPassword, ref str);
        }

        public static bool validate_user(Guid? applicationId, string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return false;

            string savedPass = string.Empty, savedSalt = string.Empty;
            UsersController.get_current_password(applicationId, username, ref savedPass, ref savedSalt);

            bool loggedIn = false;
            int failedLoginAttemptsCount = 0;

            if (string.IsNullOrEmpty(savedPass) || string.IsNullOrEmpty(savedSalt)) return false;
            password = PublicMethods.verify_string(password);

            loggedIn = UserUtilities.encode_password(password, savedSalt) == savedPass;
            failedLoginAttemptsCount = UsersController.login_attempt(applicationId, username, loggedIn);

            if (!loggedIn && 
                failedLoginAttemptsCount >= RaaiVanSettings.AllowedConsecutiveFailedLoginAttempts(applicationId))
                UsersController.lock_user(applicationId, username);

            return loggedIn;
        }
        
        public static void create_admin_user(Guid applicationId)
        {
            UsersController.create_admin_user(applicationId);
        }
    }

    public class ItemVisitsCount
    {
        private Guid? _ItemID;
        private int? _Count;

        public Guid? ItemID
        {
            get { return _ItemID; }
            set { _ItemID = value; }
        }

        public int? Count
        {
            get { return _Count; }
            set { _Count = value; }
        }
    }

    public class Password {
        private string _Value;
        private string _Salt;
        private string _Salted;
        private string _Encrypted;
        public bool? AutoGenerated;

        public Password() {
            _Salt = UserUtilities.generate_password_salt();
        }

        public Password(string value) {
            _Value = value;
            _Salt = UserUtilities.generate_password_salt();
            _set();
        }

        private void _set() {
            _Salted = UserUtilities.encode_password(_Value, _Salt);
            _Encrypted = PublicMethods.sha1(_Value);
        }

        public string Value {
            get { return _Value; }
            set {
                _Value = value;
                _set();
            }
        }

        public string Salt
        {
            get { return _Salt; }
        }

        public string Salted {
            get { return _Salted; }
        }

        public string Encrypted
        {
            get { return _Encrypted; }
        }
    }

    public class User
    {
        public Guid? UserID;
        public string UserName;
        public string Password;
        public string PasswordSalt;
        public string SaltedPassword;
        public string FirstName;
        public string LastName;
        public DateTime? Birthday;
        public string JobTitle;
        public EmploymentType EmploymentType;
        public Guid? MainPhoneID;
        public Guid? MainEmailID;
        public List<PhoneNumber> PhoneNumbers;
        public List<EmailAddress> Emails;
        public bool? IsApproved;
        public bool? IsLockedOut;
        public DateTime? LastLockoutDate;
        
        public User()
        {
            EmploymentType = EmploymentType.NotSet;
            PhoneNumbers = new List<PhoneNumber>();
            Emails = new List<EmailAddress>();
        }
        
        public string FullName {
            get {
                return ((string.IsNullOrEmpty(FirstName) ? string.Empty : FirstName) + " " +
                    (string.IsNullOrEmpty(LastName) ? string.Empty : LastName)).Trim();
            }
        }

        public string toJson(Guid? applicationId = null, bool profileImageUrl = false)
        {
            return "{\"UserID\":\"" + (UserID.HasValue ? UserID.ToString() : string.Empty) + "\"" +
                ",\"UserName\":\"" + Base64.encode(UserName) + "\"" +
                ",\"FirstName\":\"" + Base64.encode(FirstName) + "\"" +
                ",\"LastName\":\"" + Base64.encode(LastName) + "\"" +
                (string.IsNullOrEmpty(JobTitle) ? string.Empty : ",\"JobTitle\":\"" + Base64.encode(JobTitle) + "\"") +
                (!profileImageUrl || !UserID.HasValue || !applicationId.HasValue ? string.Empty :
                    ",\"ProfileImageURL\":\"" +
                        DocumentUtilities.get_personal_image_address(applicationId.Value, UserID.Value) + "\"") +
                "}";
        }
    }

    public class ApplicationUsers {
        public Guid? ApplicationID;
        public int? Count;
        public List<User> Users;

        public ApplicationUsers() {
            Users = new List<User>();
        }

        public string toJson() {
            return "{\"ApplicationID\":\"" + (!ApplicationID.HasValue ? string.Empty : ApplicationID.ToString()) + "\"" +
                ",\"TotalCount\":" + (!Count.HasValue ? 0 : Count.Value).ToString() +
                ",\"Users\":[" + string.Join(",", Users.Where(u => u != null)
                    .Select(u => u.toJson(ApplicationID, profileImageUrl: true))) + "]" +
                "}";
        }
    }

    public class Friend
    {
        private User _User;
        private DateTime _RequestDate;
        private DateTime? _AcceptionDate;
        private bool? _AreFriends;
        private bool? _IsSender;
        private int? _MutualFriendsCount;


        public Friend()
        {
            _User = new User();
        }

        public User User
        {
            get { return _User; }
            set { _User = value; }
        }

        public DateTime RequestDate
        {
            get { return _RequestDate; }
            set { _RequestDate = value; }
        }

        public DateTime? AcceptionDate
        {
            get { return _AcceptionDate; }
            set { _AcceptionDate = value; }
        }

        public bool? AreFriends
        {
            get { return _AreFriends; }
            set { _AreFriends = value; }
        }

        public bool? IsSender
        {
            get { return _IsSender; }
            set { _IsSender = value; }
        }

        public int? MutualFriendsCount
        {
            get { return _MutualFriendsCount; }
            set { _MutualFriendsCount = value; }
        }
    }
    
    public class FriendSuggestion
    {
        private User _User;
        private int? _MutualFriends;

        public FriendSuggestion()
        {
            _User = new User();
        }

        public User User
        {
            get { return _User; }
            set { _User = value; }
        }

        public int? MutualFriends
        {
            get { return _MutualFriends; }
            set { _MutualFriends = value; }
        }
    }

    public class Invitation
    {
        private Guid? _InvitationID;
        private User _SenderUser;
        private User _ReceiverUser;
        private DateTime? _SendDate;
        private string _Email;
        private bool? _Activated;

        public Invitation()
        {
            _SenderUser = new User();
            _ReceiverUser = new User();
        }

        public Guid? InvitationID
        {
            get { return _InvitationID; }
            set { _InvitationID = value; }
        }

        public User SenderUser
        {
            get { return _SenderUser; }
            set { _SenderUser = value; }
        }

        public User ReceiverUser
        {
            get { return _ReceiverUser; }
            set { _ReceiverUser = value; }
        }

        public DateTime? SendDate
        {
            get { return _SendDate; }
            set { _SendDate = value; }
        }

        public string Email
        {
            get { return _Email; }
            set { _Email = value; }
        }

        public bool? Activated
        {
            get { return _Activated; }
            set { _Activated = value; }
        }
    }
    
    public class PhoneNumber
    {
        private Guid _NumberID;
        private Guid _UserID;
        private string _Number;
        private PhoneType _PhoneType;

        public Guid NumberID
        {
            get { return _NumberID; }
            set { _NumberID = value; }
        }

        public Guid UserID
        {
            get { return _UserID; }
            set { _UserID = value; }
        }

        public string Number
        {
            get { return _Number; }
            set { _Number = value; }
        }

        public PhoneType PhoneType
        {
            get { return _PhoneType; }
            set { _PhoneType = value; }
        }
    }

    public class EmailAddress
    {
        private Guid _EmailID;
        private Guid _UserID;
        private string _EmailAddress;

        public Guid EmailID
        {
            get { return _EmailID; }
            set { _EmailID = value; }
        }

        public Guid UserID
        {
            get { return _UserID; }
            set { _UserID = value; }
        }

        public string Address
        {
            get { return _EmailAddress; }
            set { _EmailAddress = value; }
        }
    }

    public class EmailContactStatus
    {
        public Guid? UserID;
        public string Email;
        public bool? FriendRequestReceived;
    }

    public class JobExperience
    {
        private Guid? _JobId;
        private Guid? _UserId;
        private string _Title;
        private string _Employer;
        private DateTime? _StartDate;
        private DateTime? _EndDate;

        public Guid? JobID
        {
            get { return _JobId; }
            set { _JobId = value; }
        }

        public Guid? UserID
        {
            get { return _UserId; }
            set { _UserId = value; }
        }

        public string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }

        public string Employer
        {
            get { return _Employer; }
            set { _Employer = value; }
        }

        public DateTime? StartDate
        {
            get { return _StartDate; }
            set { _StartDate = value; }
        }

        public DateTime? EndDate
        {
            get { return _EndDate; }
            set { _EndDate = value; }
        }
    }

    public class EducationalExperience
    {
        private Guid? _EducationId;
        private Guid? _UserId;
        private string _School;
        private string _StudyField;
        private EducationalLevel? _Level;
        private DateTime? _StartDate;
        private DateTime? _EndDate;
        private GraduateDegree? _GraduateDegree;
        public bool? IsSchool;

        public Guid? EducationID
        {
            get { return _EducationId; }
            set { _EducationId = value; }
        }

        public Guid? UserID
        {
            get { return _UserId; }
            set { _UserId = value; }
        }

        public string School
        {
            get { return _School; }
            set { _School = value; }
        }

        public string StudyField
        {
            get { return _StudyField; }
            set { _StudyField = value; }
        }

        public EducationalLevel? Level
        {
            get { return _Level; }
            set { _Level = value; }
        }

        public DateTime? StartDate
        {
            get { return _StartDate; }
            set { _StartDate = value; }
        }

        public DateTime? EndDate
        {
            get { return _EndDate; }
            set { _EndDate = value; }
        }

        public GraduateDegree? GraduateDegree
        {
            get { return _GraduateDegree; }
            set { _GraduateDegree = value; }
        }
    }

    public class HonorsAndAwards
    {
        private Guid? _ID;
        private Guid? _UserID;
        private string _Title;
        private string _Issuer;
        private string _Occupation;
        private DateTime? _IssueDate;
        private string _Description;

        public Guid? ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        public Guid? UserID
        {
            get { return _UserID; }
            set { _UserID = value; }
        }

        public string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }

        public string Issuer
        {
            get { return _Issuer; }
            set { _Issuer = value; }
        }

        public string Occupation
        {
            get { return _Occupation; }
            set { _Occupation = value; }
        }

        public DateTime? IssueDate
        {
            get { return _IssueDate; }
            set { _IssueDate = value; }
        }

        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }
    }

    public class Language
    {
        public Guid? ID;
        public Guid? UserID;
        private Guid? _LanguageID;
        private string _LanguageName;
        private LanguageLevel? _Level;

        public Guid? LanguageID
        {
            get { return _LanguageID; }
            set { _LanguageID = value; }
        }

        public string LanguageName
        {
            get { return _LanguageName; }
            set { _LanguageName = value; }
        }

        public LanguageLevel? Level
        {
            get { return _Level; }
            set { _Level = value; }
        }
    }

    public class UserGroup
    {
        public Guid? GroupID;
        public string Title;
        public string Description;
    }

    public enum AccessRoleName
    {
        None,
        ManagementSystem,
        UsersManagement,
        ManageConfidentialityLevels,
        UserGroupsManagement,
        ContentsManagement,
        Reports,
        DataImport,
        ManageOntology,
        ManageWorkflow,
        ManageForms,
        ManagePolls,
        KnowledgeAdmin,
        SMSEMailNotifier,
        ManageQA
    }

    public class AccessRole
    {
        public Guid? RoleID;
        public AccessRoleName Name;
        public string Title;
    }

    public class AdvancedUserSearch
    {
        public Guid? UserID;
        public double? Rank;
        public int? IsMemberCount;
        public int? IsExpertCount;
        public int? IsContributorCount;
        public int? HasPropertyCount;
        public int? Resume;
    }

    public class AdvancedUserSearchMeta
    {
        public Guid? NodeID;
        public double? Rank;
        public bool? IsMember;
        public bool? IsExpert;
        public bool? IsContributor;
        public bool? HasProperty;
    }

    public class VerificationCode
    {
        protected int TimeOut;
        protected static int TotalTimeOutCoefficient = 4;

        protected Guid? _ApplicationID;
        protected string _Token;
        protected long _Code;
        protected DateTime? _ExpirationDate;
        protected string _EmailAddress;
        protected string _PhoneNumber;
        protected string _Media;
        protected int _TTL;

        protected static Dictionary<string, VerificationCode> Tokens = new Dictionary<string, VerificationCode>();

        public VerificationCode(Guid? applicationId, string emailAddress, string phoneNumber)
        {
            TimeOut = RaaiVanSettings.Users.TwoStepAuthenticationTimeout(applicationId);

            _ApplicationID = applicationId;
            _Token = UserUtilities.generate_password(20);
            reset_code();
            _EmailAddress = emailAddress;
            _PhoneNumber = phoneNumber;
            _TTL = 3;
        }

        protected bool update_ttl()
        {
            if (_TTL <= 0) return false;
            else {
                --_TTL;
                return true;
            }
        }

        protected void reset_code() { _Code = PublicMethods.get_random_number(5); }

        protected void reset_expiration_date()
        {
            _ExpirationDate = DateTime.Now.AddSeconds(TimeOut + 10);
        }
        
        public string Token { get { return _Token; } }
        public long Code { get { return _Code; } }
        
        protected bool use()
        {
            if (_ExpirationDate.HasValue) return false;

            Tokens[_Token] = this;

            reset_expiration_date();
            
            Task task = Task.Delay((TimeOut + 10) * 1000 * TotalTimeOutCoefficient)
                .ContinueWith(t => { lock (Tokens) { if (Tokens.ContainsKey(_Token)) Tokens.Remove(_Token); } });

            return true;
        }

        public bool send_code()
        {
            bool result = send_email() || send_sms();
            if (result) use();
            return result;
        }

        protected bool resend()
        {
            reset_expiration_date();
            reset_code();

            return _Media == "Email" ? send_email() : (_Media == "SMS" ? send_sms() : false);
        }

        protected virtual void email_body_subject(ref string body, ref string subject)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("Code", _Code.ToString());

            body = EmailTemplates.get_email_template(!_ApplicationID.HasValue ? Guid.Empty : _ApplicationID.Value, 
                EmailTemplateType.ConfirmationCode, dic);
            subject = EmailTemplates.get_email_subject_template(!_ApplicationID.HasValue ? Guid.Empty : _ApplicationID.Value, 
                EmailTemplateType.ConfirmationCode, dic);
        }

        protected virtual string sms_body()
        {
            return "کد تایید شما در رای ون " + _Code.ToString() + " است.";
        }

        protected bool send_email()
        {
            if (string.IsNullOrEmpty(_EmailAddress)) return false;

            string emailBody = string.Empty, emailSubject = string.Empty;

            email_body_subject(ref emailBody, ref emailSubject);

            bool result = PublicMethods.send_email(_ApplicationID, _EmailAddress, emailSubject, emailBody);
            if (result) _Media = "Email";

            return result;
        }
        
        protected bool send_sms()
        {
            if (string.IsNullOrEmpty(_PhoneNumber)) return false;

            bool result = PublicMethods.send_sms(_PhoneNumber, sms_body());
            if (result) _Media = "SMS";

            return result;
        }

        public string toJson()
        {
            return "{\"Token\":\"" + _Token + "\"" +
                ",\"Media\":\"" + _Media + "\"" +
                ",\"EmailAddress\":\"" + Base64.encode(_EmailAddress) + "\"" +
                ",\"PhoneNumber\":\"" + Base64.encode(_PhoneNumber) + "\"" +
                ",\"Timeout\":" + TimeOut.ToString() +
                ",\"TotalTimeout\":" + (TimeOut * (TotalTimeOutCoefficient - 1)).ToString() +
                "}";
        }

        public static bool resend_code(string token)
        {
            return !string.IsNullOrEmpty(token) && Tokens.ContainsKey(token) && Tokens[token].resend();
        }

        public static VerificationCode validate(string token, long code, ref bool disposed)
        {
            disposed = false;
            if (!Tokens.ContainsKey(token)) return null;

            VerificationCode t = Tokens[token];

            if (!t.update_ttl())
            {
                disposed = true;
                return null;
            }
            
            if (t._Code == code && t._ExpirationDate.HasValue && t._ExpirationDate.Value > DateTime.Now) return t;
            else return null;
        }

        public static bool process_request(Guid applicationId, string emailAddress, string phoneNumber, 
            string token, long? code, ref string responseText) {
            bool disposed = false;

            if (string.IsNullOrEmpty(token) || !code.HasValue)
            {
                VerificationCode vc = new VerificationCode(applicationId, emailAddress, phoneNumber);

                if (vc.send_code())
                    responseText = "{\"VerificationCode\":" + vc.toJson() + "}";
                else responseText = "{\"ErrorText\":\"" + Messages.SendingVerificationCodeFailed.ToString() + "\"}";

                return false;
            }
            else if (VerificationCode.validate(token, code.Value, ref disposed) == null)
            {
                responseText = "{\"ErrorText\":\"" + Messages.VerificationCodeDidNotMatch.ToString() + "\"" +
                    ",\"CodeDisposed\":" + disposed.ToString().ToLower() + "}";
                return false;
            }

            return true;
        }
    }

    public class TwoStepAuthenticationToken : VerificationCode
    {
        private Guid? _UserID;
        private bool? _WasNormalUserPassLogin;

        public Guid? UserID { get { return _UserID; } }

        protected override void email_body_subject(ref string body, ref string subject)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("Code", _Code.ToString());

            body = EmailTemplates.get_email_template(!_ApplicationID.HasValue ? Guid.Empty : _ApplicationID.Value, 
                EmailTemplateType.TwoStepAuthenticationCode, dic);
            subject = EmailTemplates.get_email_subject_template(!_ApplicationID.HasValue ? Guid.Empty : _ApplicationID.Value, 
                EmailTemplateType.TwoStepAuthenticationCode, dic);
        }

        protected override string sms_body()
        {
            return "کد احراز هویت شما در رای ون " + _Code.ToString() + " است.";  
        }

        public TwoStepAuthenticationToken(Guid? applicationId, Guid userId, bool wasNormalUserPassLogin, 
            string emailAddress, string phoneNumber) : base(applicationId, emailAddress, phoneNumber)
        {
            _UserID = userId;
            _WasNormalUserPassLogin = wasNormalUserPassLogin;
        }

        public bool WasNormalUserPassLogin { get { return _WasNormalUserPassLogin.HasValue && _WasNormalUserPassLogin.Value; } }
    }

    public class RemoteServer
    {
        public Guid? ID;
        public Guid? UserID;
        public string Name;
        public string URL;
        public string UserName;
        public string Passwrod;

        public byte[] get_password_encrypted()
        {
            return DocumentUtilities.encrypt_bytes_aes_native(
                Encoding.UTF8.GetBytes(string.IsNullOrEmpty(Passwrod) ? string.Empty : Passwrod));
        }

        public void set_password_encrypted(byte[] value)
        {
            Passwrod = value == null ? string.Empty :
                Encoding.UTF8.GetString(DocumentUtilities.decrypt_bytes_aes_native(value));
        }

        public string toJson()
        {
            return "{\"ID\":" + (!ID.HasValue ? "null" : "\"" + ID.Value.ToString() + "\"") +
                ",\"UserID\":" + (!UserID.HasValue ? "null" : "\"" + UserID.Value.ToString() + "\"") +
                ",\"Name\":\"" + (string.IsNullOrEmpty(Name) ? string.Empty : Base64.encode(Name)) + "\"" +
                ",\"URL\":\"" + (string.IsNullOrEmpty(URL) ? string.Empty : Base64.encode(URL)) + "\"" +
                ",\"UserName\":\"" + (string.IsNullOrEmpty(UserName) ? string.Empty : Base64.encode(UserName)) + "\"" +
                ",\"Password\":\"" + (string.IsNullOrEmpty(Passwrod) ? string.Empty : Base64.encode(Passwrod)) + "\"" +
                "}";
        }
    }
}
