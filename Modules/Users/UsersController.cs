﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web.Security;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Modules.Users
{
    public static class UsersController
    {
        public static List<Password> get_last_passwords(Guid userId, bool? autoGenerated, int? count)
        {
            return DataProvider.GetLastPasswords(userId, autoGenerated, count);
        }

        public static DateTime? get_last_password_date(Guid userId)
        {
            return DataProvider.GetLastPasswordDate(userId);
        }

        public static void get_current_password(Guid userId, ref string password, ref string passwordSalt)
        {
            DataProvider.GetCurrentPassword(userId, ref password, ref passwordSalt);
        }

        public static void get_current_password(Guid? applicationId, string username,
            ref string password, ref string passwordSalt)
        {
            Guid? userId = UsersController.get_user_id(applicationId, username);
            if(userId.HasValue) UsersController.get_current_password(userId.Value, ref password, ref passwordSalt);
        }

        public static bool create_user(Guid? applicationId, User info, bool passAutoGenerated)
        {
            return DataProvider.CreateUser(applicationId, info, passAutoGenerated);
        }

        public static bool create_temporary_user(Guid? applicationId, User info, bool passAutoGenerated,
            string emailSubject, string confirmationEmailBody, DateTime? expirationDate, string activationCode,
            Guid? invitationId, ref string errorMessage)
        {
            return DataProvider.CreateTemporaryUser(applicationId, info, passAutoGenerated, 
                emailSubject, confirmationEmailBody, expirationDate, activationCode, invitationId, ref errorMessage);
        }

        public static bool activate_temporary_user(Guid? applicationId, string activationCode, ref string errorMessage)
        {
            return DataProvider.ActivateTemporaryUser(applicationId, activationCode, ref errorMessage);
        }

        public static Guid? get_invitation_id(Guid applicationId, string email, bool checkIfNotUsed)
        {
            return DataProvider.GetInvitationID(applicationId, email, checkIfNotUsed);
        }

        public static bool is_invited(Guid applicationId, string email)
        {
            return UsersController.get_invitation_id(applicationId, email, false).HasValue;
        }

        public static bool invite_user(Guid applicationId, Guid invitationId, string email,
            bool isExistingInvitation, Guid currentUserId, string emailSubject, string emailBody)
        {
            return DataProvider.InviteUser(applicationId, invitationId, email, 
                isExistingInvitation, currentUserId, emailSubject, emailBody);
        }

        public static int get_invited_users_count(Guid applicationId, Guid? invitorUserId)
        {
            return DataProvider.GetInvitedUsersCount(applicationId, invitorUserId);
        }

        public static List<Invitation> get_user_invitations(Guid applicationId, 
            Guid? senderUserId, int? count, long? lowerBoundary, ref long totalCount)
        {
            List<Invitation> retInvitations = new List<Invitation>();
            DataProvider.GetUserInvitations(applicationId, 
                ref retInvitations, senderUserId, count, lowerBoundary, ref totalCount);
            return retInvitations;
        }

        public static Guid? get_invitation_application_id(Guid invitationId, bool checkIfNotUsed)
        {
            return DataProvider.GetInvitationApplicationID(invitationId, checkIfNotUsed);
        }

        public static bool set_pass_reset_ticket(Guid? applicationId, Guid userId, Guid ticket, 
            string email, string emailSubject, string emailBody, bool sendEmail = true)
        {
            return DataProvider.SetPassResetTicket(applicationId, userId, ticket, email, emailSubject, emailBody, sendEmail);
        }

        public static bool set_pass_reset_ticket(Guid applicationId, string username, Guid ticket, 
            string email, string emailSubject, string emailBody, bool sendEmail = true)
        {
            Guid? userId = UsersController.get_user_id(applicationId, username);
            return userId.HasValue && set_pass_reset_ticket(applicationId, userId.Value, ticket, email, emailSubject, emailBody, sendEmail);
        }

        public static Guid? get_pass_reset_ticket(Guid userId)
        {
            return DataProvider.GetPassResetTicket(userId);
        }

        public static Guid? get_pass_reset_ticket(Guid applicationId, string username)
        {
            Guid? userId = UsersController.get_user_id(applicationId, username);
            return !userId.HasValue ? null : get_pass_reset_ticket(userId.Value);
        }
        
        public static bool set_password(Guid? applicationId, Guid userId, Guid ticket, 
            string password, bool ignorePasswordPolicy, bool autoGenerated, ref string errorMessage)
        {
            if (!ignorePasswordPolicy && !UserUtilities.check_password_policy(applicationId, password, null, ref errorMessage)) return false;

            if (ticket == Guid.Empty || ticket != UsersController.get_pass_reset_ticket(userId))
            {
                errorMessage = Messages.PasswordResetTicketIsNotValid.ToString();
                return false;
            }

            return UsersController.set_password(applicationId, userId, 
                password, ignorePasswordPolicy, autoGenerated, ref errorMessage);
        }
        
        public static int get_users_count(Guid applicationId, 
            DateTime? creationDateLowerThreshold = null, DateTime? creationDateUpperThreshold = null)
        {
            return DataProvider.GetUsersCount(applicationId, creationDateLowerThreshold, creationDateUpperThreshold);
        }

        public static List<Guid> get_user_ids(Guid? applicationId, ref List<string> usernames)
        {
            List<Guid> lst = new List<Guid>();
            DataProvider.GetUserIDs(applicationId, ref lst, ref usernames);
            return lst;
        }

        public static Guid? get_user_id(Guid? applicationId, string username)
        {
            List<string> uns = new List<string>();
            uns.Add(username);
            List<Guid> ids = get_user_ids(applicationId, ref uns);
            return ids != null && ids.Count == 1 ? (Guid?)ids[0] : null;
        }

        public static List<User> get_users(Guid applicationId, string searchText, 
            long? lowerBoundary, int? count, bool? searchAll, bool? isOnline, ref long totalCount)
        {
            List<User> lst = new List<User>();
            DataProvider.GetUsers(applicationId, ref lst, searchText, lowerBoundary, count, 
                searchAll, isOnline, ref totalCount);
            return lst;
        }

        public static List<User> get_users(Guid applicationId, string searchText, long? lowerBoundary = null, 
            int? count = null, bool? searchAll = null, bool? isOnline = null)
        {
            List<User> lst = new List<User>();
            long totalCount = 0;
            DataProvider.GetUsers(applicationId, ref lst, searchText, lowerBoundary, count, 
                searchAll, isOnline, ref totalCount);
            return lst;
        }

        public static List<User> get_users(Guid? applicationId, List<Guid> userIds)
        {
            List<User> lst = new List<User>();
            DataProvider.GetUsers(applicationId, ref lst, userIds);
            return lst;
        }

        public static User get_user(Guid? applicationId, Guid userId)
        {
            return get_users(applicationId, new List<Guid>() { userId }).FirstOrDefault();
        }

        public static List<User> get_users(Guid? applicationId, List<string> usernames)
        {
            List<Guid> userIds = UsersController.get_user_ids(applicationId, ref usernames);
            List<User> lst = new List<User>();
            DataProvider.GetUsers(applicationId, ref lst, userIds);
            return lst;
        }

        public static User get_user(Guid applicationId, string username)
        {
            return get_users(applicationId, new List<string>() { username }).FirstOrDefault();
        }

        public static List<ApplicationUsers> get_application_users_partitioned(List<Guid> applicationIds, int? count)
        {
            return DataProvider.GetApplicationUsersPartitioned(applicationIds, count);
        }

        public static List<AdvancedUserSearch> advanced_user_search(Guid applicationId, string searchText,
            List<Guid> nodeTypeIds, List<Guid> nodeIds, bool? members, bool? experts, bool? contributors, 
            bool? propertyOwners, bool? resume, long? lowerBoundary, int? count, ref long totalCount)
        {
            List<AdvancedUserSearch> lst = new List<AdvancedUserSearch>();
            DataProvider.AdvancedUserSearch(applicationId, ref lst, searchText, nodeTypeIds, nodeIds, members,
                experts, contributors, propertyOwners, resume, lowerBoundary, count, ref totalCount);
            return lst;
        }

        public static List<AdvancedUserSearchMeta> advanced_user_search_meta(Guid applicationId, 
            Guid userId, string searchText, List<Guid> nodeTypeIds, List<Guid> nodeIds, 
            bool? members, bool? experts, bool? contributors, bool? propertyOwners)
        {
            List<AdvancedUserSearchMeta> lst = new List<Users.AdvancedUserSearchMeta>();
            DataProvider.AdvancedUserSearchMeta(applicationId, ref lst, userId, searchText,
                nodeTypeIds, nodeIds, members, experts, contributors, propertyOwners);
            return lst;
        }

        public static User get_system_user(Guid? applicationId)
        {
            return DataProvider.GetSystemUser(applicationId);
        }

        public static bool create_admin_user(Guid applicationId)
        {
            return DataProvider.CreateAdminUser(applicationId);
        }

        public static List<string> get_not_existing_users(Guid applicationId, ref List<string> usernames)
        {
            List<string> lst = new List<string>();
            DataProvider.GetNotExistingUsers(applicationId, ref lst, ref usernames);
            return lst;
        }

        public static List<Guid> get_departments_user_ids(Guid applicationId, ref List<Guid> departmentIds)
        {
            List<Guid> lst = new List<Guid>();
            DataProvider.GetDepartmentsUserIDs(applicationId, ref lst, ref departmentIds);
            return lst;
        }

        public static List<Guid> get_department_user_ids(Guid applicationId, Guid departmentId)
        {
            List<Guid> _uIds = new List<Guid>();
            _uIds.Add(departmentId);
            return get_departments_user_ids(applicationId, ref _uIds);
        }

        public static List<Guid> get_department_groups_user_ids(Guid applicationId, ref List<Guid> departmentGroupIds)
        {
            List<Guid> lst = new List<Guid>();
            DataProvider.GetDepartmentGroupsUserIDs(applicationId, ref lst, ref departmentGroupIds);
            return lst;
        }

        public static List<Guid> get_department_group_user_ids(Guid applicationId, Guid departmentGroupId)
        {
            List<Guid> _uIds = new List<Guid>();
            _uIds.Add(departmentGroupId);
            return get_department_groups_user_ids(applicationId, ref _uIds);
        }

        public static bool register_item_visit(Guid applicationId, 
            Guid itemId, Guid userId, DateTime visitDate, VisitItemTypes itemType)
        {
            return DataProvider.RegisterItemVisit(applicationId, itemId, userId, visitDate, itemType);
        }

        public static List<ItemVisitsCount> get_items_visits_count(Guid applicationId, ref List<Guid> itemIds, 
            DateTime? lowerDateLimit = null, DateTime? upperDateLimit = null)
        {
            List<ItemVisitsCount> lst = new List<ItemVisitsCount>();
            DataProvider.GetItemsVisitsCount(applicationId, ref lst, ref itemIds, lowerDateLimit, upperDateLimit);
            return lst;
        }

        public static ItemVisitsCount get_item_visits_count(Guid applicationId, 
            Guid itemId, DateTime? lowerDateLimit = null, DateTime? upperDateLimit = null)
        {
            List<Guid> _ids = new List<Guid>();
            _ids.Add(itemId);
            return get_items_visits_count(applicationId, ref _ids, lowerDateLimit, upperDateLimit).FirstOrDefault();
        }

        public static List<ItemVisitsCount> get_most_visited_items(Guid applicationId, VisitItemTypes itemType, 
            int? count, DateTime? lowerDateLimit, DateTime? upperDateLimit)
        {
            List<ItemVisitsCount> lst = new List<ItemVisitsCount>();
            DataProvider.GetMostVisitedItems(applicationId,
                ref lst, itemType, count, lowerDateLimit, upperDateLimit, null, null);
            return lst;
        }

        public static List<ItemVisitsCount> get_most_visited_items(Guid applicationId, VisitItemTypes itemType,
            int? count, DateTime? lowerDateLimit, DateTime? upperDateLimit, Guid nodeTypeId)
        {
            List<ItemVisitsCount> lst = new List<ItemVisitsCount>();
            DataProvider.GetMostVisitedItems(applicationId,
                ref lst, itemType, count, lowerDateLimit, upperDateLimit, nodeTypeId, null);
            return lst;
        }

        public static List<ItemVisitsCount> get_most_visited_items(Guid applicationId, VisitItemTypes itemType,
            int? count, DateTime? lowerDateLimit, DateTime? upperDateLimit, int knowledgeTypeId)
        {
            List<ItemVisitsCount> lst = new List<ItemVisitsCount>();
            DataProvider.GetMostVisitedItems(applicationId,
                ref lst, itemType, count, lowerDateLimit, upperDateLimit, null, knowledgeTypeId);
            return lst;
        }

        public static bool send_friendship_request(Guid applicationId, Guid userId, Guid receiverUserId)
        {
            return DataProvider.SendFriendshipRequest(applicationId, userId, receiverUserId);
        }

        public static bool accept_friendship(Guid applicationId, Guid userId, Guid senderUserId)
        {
            return DataProvider.AcceptFriendship(applicationId, userId, senderUserId);
        }

        public static bool reject_friendship(Guid applicationId, Guid userId, Guid friendUserId)
        {
            return DataProvider.RejectFriendship(applicationId, userId, friendUserId);
        }

        public static List<Friend> get_friendship_status(Guid applicationId, 
            Guid userId, List<Guid> otherUserIds, bool? mutualsCount = false)
        {
            List<Friend> retList = new List<Friend>();
            DataProvider.GetFriendshipStatus(applicationId, ref retList, userId, otherUserIds, mutualsCount);
            return retList;
        }

        public static Friend get_friendship_status(Guid applicationId, 
            Guid userId, Guid otherUserId, bool? mutualsCount = false)
        {
            List<Guid> uIds = new List<Guid>();
            uIds.Add(otherUserId);
            return get_friendship_status(applicationId, userId, uIds, mutualsCount).FirstOrDefault();
        }

        public static List<Guid> get_friend_ids(Guid applicationId, 
            Guid userId, bool? areFriends = null, bool? sent = null, bool? received = null)
        {
            List<Guid> friendIds = new List<Guid>();
            DataProvider.GetFriendIDs(applicationId, ref friendIds, userId, areFriends, sent, received);
            return friendIds;
        }

        public static List<Friend> get_friends(Guid applicationId, Guid userId, List<Guid> friendIds, 
            bool? mutualsCount, int? count, long? lowerBoundary, ref long totalCount, 
            bool? areFriends = null, bool? isSender = null, string searchText = null)
        {
            List<Friend> friends = new List<Friend>();
            DataProvider.GetFriends(applicationId, ref friends, userId, friendIds, mutualsCount, 
                areFriends, isSender, searchText, count, lowerBoundary, ref totalCount);
            return friends;
        }

        public static List<Friend> get_friends(Guid applicationId, Guid userId, bool? mutualsCount, 
            int? count, long? lowerBoundary, ref long totalCount, 
            bool? areFriends = null, bool? isSender = null, string searchText = null)
        {
            return get_friends(applicationId, userId, new List<Guid>(), mutualsCount, 
                count, lowerBoundary, ref totalCount, areFriends, isSender, searchText);
        }

        public static int get_friends_count(Guid applicationId, 
            Guid userId, bool? areFriends = null, bool? sent = null, bool? received = null)
        {
            return DataProvider.GetFriendsCount(applicationId, userId, areFriends, sent, received);
        }

        public static List<EmailContactStatus> get_email_contacts_status(Guid applicationId, 
            Guid userId, List<string> emails, bool? saveEmails = null)
        {
            List<EmailContactStatus> retList = new List<EmailContactStatus>();
            DataProvider.GetEmailContactsStatus(applicationId, ref retList, userId, emails, saveEmails);
            return retList;
        }

        public static bool update_friend_suggestions(Guid applicationId, Guid? userId = null)
        {
            return DataProvider.UpdateFriendSuggestions(applicationId, userId);
        }
        
        public static List<FriendSuggestion> get_friend_suggestions(Guid applicationId, Guid userId, 
            int? count, long? lowerBoundary, ref long totalCount)
        {
            List<FriendSuggestion> frndSuggestions = new List<FriendSuggestion>();
            DataProvider.GetFriendSuggestions(applicationId,
                ref frndSuggestions, userId, count, lowerBoundary, ref totalCount);
            return frndSuggestions;
        }
        
        public static bool set_theme(Guid? applicationId, Guid userId, string theme)
        {
            return DataProvider.SetTheme(applicationId, userId, theme);
        }

        public static string get_theme(Guid? applicationId, Guid userId)
        {
            return DataProvider.GetTheme(applicationId, userId);
        }

        public static List<Guid> get_approved_user_ids(Guid applicationId, List<Guid> userIds)
        {
            List<Guid> retIds = new List<Guid>();
            DataProvider.GetApprovedUserIDs(applicationId, ref retIds, ref userIds);
            return retIds;
        }

        public static bool set_last_activity_date(Guid userId)
        {
            return DataProvider.SetLastActivityDate(userId);
        }

        public static bool add_or_modify_remote_server(Guid applicationId, Guid serverId, Guid userId,
            string name, string url, string username, byte[] password)
        {
            return DataProvider.AddOrModifyRemoteServer(applicationId, serverId, userId, name, url, username, password);
        }

        public static bool remove_remote_server(Guid applicationId, Guid serverId)
        {
            return DataProvider.RemoveRemoteServer(applicationId, serverId);
        }

        public static List<RemoteServer> get_remote_servers(Guid applicationId)
        {
            return DataProvider.GetRemoteServers(applicationId, null);
        }

        public static RemoteServer get_remote_server(Guid applicationId, Guid serverId)
        {
            List<RemoteServer> items = serverId == Guid.Empty ? new List<RemoteServer>() :
                DataProvider.GetRemoteServers(applicationId, serverId);
            return items.Count > 0 ? items[0] : null;
        }

        /* Profile */

        public static bool set_first_and_last_name(Guid? applicationId, Guid userId, string firstName, string lastName)
        {
            return DataProvider.SetFirstAndLastName(applicationId, userId, firstName, lastName);
        }

        public static bool set_username(Guid applicationId, Guid userId, string userName)
        {
            return DataProvider.SetUserName(applicationId, userId, userName);
        }

        public static bool set_password(Guid? applicationId, Guid userId, 
            string password, bool ignorePasswordPolicy, bool autoGenerated, ref string errorMessage)
        {
            if (!ignorePasswordPolicy && 
                !UserUtilities.check_password_policy(applicationId, password, null, ref errorMessage)) return false;

            return DataProvider.SetPassword(userId, password, autoGenerated);
        }

        public static bool set_password(Guid? applicationId, string username, string password,
            bool ignorePasswordPolicy, bool autoGenerated, ref string errorMessage)
        {
            Guid? userId = UsersController.get_user_id(applicationId, username);

            return userId.HasValue && UsersController.set_password(applicationId, userId.Value, password, 
                ignorePasswordPolicy, autoGenerated, ref errorMessage);
        }

        public static bool lock_user(Guid userId)
        {
            User u = new User();
            return DataProvider.Locked(userId, true, ref u);
        }

        public static bool lock_user(Guid? applicationId, string username)
        {
            Guid? userId = UsersController.get_user_id(applicationId, username);
            User u = new User();
            return userId.HasValue && DataProvider.Locked(userId.Value, true, ref u);
        }

        public static bool unlock_user(Guid userId)
        {
            User u = new User();
            return DataProvider.Locked(userId, false, ref u);
        }

        public static bool unlock_user(Guid? applicationId, string username)
        {
            Guid? userId = UsersController.get_user_id(applicationId, username);
            User u = new User();
            return userId.HasValue && DataProvider.Locked(userId.Value, false, ref u);
        }

        public static bool locked(Guid userId, ref User theUser)
        {
            return DataProvider.Locked(userId, null, ref theUser);
        }

        public static bool locked(Guid? applicationId, string username, ref User theUser)
        {
            Guid? userId = UsersController.get_user_id(applicationId, username);
            return userId.HasValue && DataProvider.Locked(userId.Value, null, ref theUser);
        }

        public static int login_attempt(Guid userId, bool succeed)
        {
            return DataProvider.LoginAttempt(userId, succeed);
        }

        public static int login_attempt(Guid? applicationId, string username, bool succeed)
        {
            Guid? userId = UsersController.get_user_id(applicationId, username);
            return !userId.HasValue ? 0 : DataProvider.LoginAttempt(userId.Value, succeed);
        }

        public static bool is_approved(Guid applicationId, Guid userId, bool? isApproved = null)
        {
            return DataProvider.IsApproved(applicationId, userId, isApproved);
        }

        public static bool set_job_title(Guid applicationId, Guid userId, string jobTitle)
        {
            return DataProvider.SetJobTitle(applicationId, userId, jobTitle);
        }

        public static bool set_employment_type(Guid applicationId, Guid userId, EmploymentType employmentType)
        {
            return DataProvider.SetEmploymentType(applicationId, userId, employmentType);
        }

        public static bool set_birthday(Guid applicationId, Guid userId, DateTime birthday)
        {
            return DataProvider.SetBirthday(applicationId, userId, birthday);
        }

        public static bool set_phone_number(Guid numberId, Guid userId, string phoneNumber, PhoneType phoneNumberType, Guid currentUserId)
        {
            return DataProvider.SetPhoneNumber(numberId, userId, phoneNumber, phoneNumberType, currentUserId);
        }

        public static bool edit_phone_number(Guid numberID, string phoneNumber, PhoneType phoneType, Guid currentUserId)
        {
            return DataProvider.EditPhoneNumber(numberID, phoneNumber, phoneType, currentUserId);
        }

        public static bool remove_phone_number(Guid numberID, Guid currentUserId)
        {
            return DataProvider.RemovePhoneNumber(numberID, currentUserId);
        }

        public static bool set_main_phone(Guid numberId, Guid userId)
        {
            return DataProvider.SetMainPhone(numberId, userId);
        }

        public static bool set_email_address(Guid emailId, Guid userId, string address, Guid currentUserId)
        {
            return DataProvider.SetEmailAddress(emailId, userId, address, currentUserId);
        }

        public static bool edit_email_address(Guid emailId, string address, Guid currentUserId)
        {
            return DataProvider.EditEmailAddress(emailId, address, currentUserId);
        }

        public static bool remove_email_address(Guid emailId, Guid currentUserId)
        {
            return DataProvider.RemoveEmailAddress(emailId, currentUserId);
        }

        public static bool set_main_email(Guid emailId, Guid userId)
        {
            return DataProvider.SetMainEmail(emailId, userId);
        }

        public static List<PhoneNumber> get_phone_numbers(Guid userId)
        {
            List<PhoneNumber> lst = new List<PhoneNumber>();
            DataProvider.GetPhoneNumbers(ref lst, userId);
            return lst;
        }

        public static PhoneNumber get_phone_number(Guid numberId)
        {
            return DataProvider.GetPhoneNumber(numberId);
        }

        public static Guid? get_main_phone(Guid userId)
        {
            return DataProvider.GetMainPhone(userId);
        }

        public static List<EmailAddress> get_email_addresses(Guid userId)
        {
            List<EmailAddress> lst = new List<EmailAddress>();
            DataProvider.GetEmailAddresses(ref lst, userId);
            return lst;
        }

        public static EmailAddress get_email_address(Guid emailId)
        {
            return DataProvider.GetEmailAddress(emailId);
        }

        public static List<string> get_not_existing_emails(Guid applicationId, List<string> emails)
        {
            List<string> lst = new List<string>();
            DataProvider.GetNotExistingEmails(applicationId, ref lst, ref emails);
            return lst;
        }

        public static bool email_exists(Guid applicationId, string email)
        {
            List<string> lst = new List<string>();
            lst.Add(email);
            return get_not_existing_emails(applicationId, lst).Count == 0;
        }

        public static List<EmailAddress> get_email_owners(Guid? applicationId, List<string> emails)
        {
            List<EmailAddress> emailsList = new List<EmailAddress>();
            DataProvider.GetEmailOwners(applicationId, ref emailsList, emails);
            return emailsList;
        }

        public static Guid get_email_owner_id(Guid? applicationId, string email)
        {
            List<string> emails = new List<string>();
            emails.Add(email);
            List<EmailAddress> retEmailsList = new List<EmailAddress>();
            return get_email_owners(applicationId, emails).Select(e => e.UserID).ToList<Guid>().FirstOrDefault();
        }

        public static Guid? get_main_email(Guid userId)
        {
            return DataProvider.GetMainEmail(userId);
        }

        public static List<PhoneNumber> get_users_main_phone(List<Guid> userIds)
        {
            List<PhoneNumber> lst = new List<PhoneNumber>();
            DataProvider.GetUsersMainPhone(ref lst, userIds);
            return lst;
        }

        public static List<EmailAddress> get_users_main_email(List<Guid> userIds)
        {
            List<EmailAddress> lst = new List<EmailAddress>();
            DataProvider.GetUsersMainEmail(ref lst, userIds);
            return lst;
        }

        public static List<JobExperience> get_job_experiences(Guid applicationId, Guid userId)
        {
            List<JobExperience> retJobs = new List<JobExperience>();
            DataProvider.GetJobExperiences(applicationId, userId, ref retJobs);
            return retJobs;
        }

        public static JobExperience get_job_experience(Guid applicationId, Guid id)
        {
            return DataProvider.GetJobExperience(applicationId, id);
        }

        public static List<EducationalExperience> get_educational_experiences(Guid applicationId, Guid userId)
        {
            List<EducationalExperience> retEducations = new List<EducationalExperience>();
            DataProvider.GetEducationalExperiences(applicationId, userId, ref retEducations);
            return retEducations;
        }

        public static EducationalExperience get_educational_experience(Guid applicationId, Guid id)
        {
            return DataProvider.GetEducationalExperience(applicationId, id);
        }

        public static List<HonorsAndAwards> get_honors_and_awards(Guid applicationId, Guid userId)
        {
            List<HonorsAndAwards> retHonors = new List<HonorsAndAwards>();
            DataProvider.GetHonorsAndAwards(applicationId, userId, ref retHonors);
            return retHonors;
        }

        public static HonorsAndAwards get_honor_or_award(Guid applicationId, Guid id)
        {
            return DataProvider.GetHonorOrAward(applicationId, id);
        }

        public static List<Language> get_user_languages(Guid applicationId, Guid userId)
        {
            List<Language> retLanguages = new List<Language>();
            DataProvider.GetUserLanguages(applicationId, userId, ref retLanguages);
            return retLanguages;
        }

        public static Language get_user_language(Guid applicationId, Guid id)
        {
            return DataProvider.GetUserLanguage(applicationId, id);
        }

        public static List<Language> get_languages(Guid applicationId)
        {
            List<Language> lst = new List<Language>();
            DataProvider.GetLanguages(applicationId, ref lst, null, null);
            return lst;
        }

        public static bool set_job_experience(Guid applicationId, Guid jobId, Guid userId, Guid currentUserId,
            string title, string employer, DateTime? startDate, DateTime? endDate)
        {
            return DataProvider.SetJobExperience(applicationId, 
                jobId, userId, currentUserId, title, employer, startDate, endDate);
        }

        public static bool set_educational_experience(Guid applicationId, Guid educationId, Guid userId, 
            Guid currentUserId, string school, string studyField, EducationalLevel? degree, GraduateDegree? level, 
            DateTime? startDate, DateTime? endDate, bool isSchool)
        {
            return DataProvider.SetEducationalExperience(applicationId, educationId, userId,
                currentUserId, school, studyField, degree, level, startDate, endDate, isSchool);
        }

        public static bool set_honor_and_award(Guid applicationId, Guid honorId, Guid userId, Guid currentUserId,
            string title, string occupation, string issuer, DateTime? issueDate, string description)
        {
            return DataProvider.SetHonorAndAward(applicationId, 
                honorId, userId, currentUserId, title, occupation, issuer, issueDate, description);
        }

        public static bool set_language(Guid applicationId, 
            Guid Id, string languageName, Guid userId, Guid currentUserId, LanguageLevel langLevel)
        {
            return DataProvider.SetLanguage(applicationId, Id, languageName, userId, currentUserId, langLevel);
        }

        public static bool remove_job_experience(Guid applicationId, Guid jobId)
        {
            return DataProvider.RemoveJobExperience(applicationId, jobId);
        }

        public static bool remove_education_experience(Guid applicationId, Guid educationId)
        {
            return DataProvider.RemoveEducationalExperience(applicationId, educationId);
        }

        public static bool remove_honor_and_award(Guid applicationId, Guid honorId)
        {
            return DataProvider.RemoveHonorAndAward(applicationId, honorId);
        }

        public static bool remove_language(Guid applicationId, Guid languageId)
        {
            return DataProvider.RemoveLanguage(applicationId, languageId);
        }

        /* end of Profile */

        /* User Groups */

        public static bool create_user_group(Guid applicationId, Guid groupId,
            string title, string description, Guid currentUserId)
        {
            return DataProvider.CreateUserGroup(applicationId, groupId, title, description, currentUserId);
        }

        public static bool modify_user_group(Guid applicationId, Guid groupId,
            string title, string description, Guid currentUserId)
        {
            return DataProvider.ModifyUserGroup(applicationId, groupId, title, description, currentUserId);
        }

        public static bool remove_user_group(Guid applicationId, Guid groupId, Guid currentUserId)
        {
            return DataProvider.RemoveUserGroup(applicationId, groupId, currentUserId);
        }

        public static bool add_user_group_members(Guid applicationId, Guid groupId, List<Guid> userIds, Guid currentUserId)
        {
            return DataProvider.AddUserGroupMembers(applicationId, groupId, userIds, currentUserId);
        }

        public static bool add_user_group_member(Guid applicationId, Guid groupId, Guid userId, Guid currentUserId)
        {
            return add_user_group_members(applicationId, groupId, new List<Guid>() { userId }, currentUserId);
        }

        public static bool remove_user_group_members(Guid applicationId, Guid groupId, 
            List<Guid> userIds, Guid currentUserId)
        {
            return DataProvider.RemoveUserGroupMembers(applicationId, groupId, userIds, currentUserId);
        }

        public static bool remove_user_group_member(Guid applicationId, Guid groupId, Guid userId, Guid currentUserId)
        {
            return remove_user_group_members(applicationId, groupId, new List<Guid>() { userId }, currentUserId);
        }

        public static bool set_user_group_permission(Guid applicationId, Guid groupId, Guid roleId, Guid currentUserId)
        {
            return DataProvider.SetUserGroupPermission(applicationId, groupId, roleId, currentUserId);
        }

        public static bool unset_user_group_permission(Guid applicationId, Guid groupId, Guid roleId, Guid currentUserId)
        {
            return DataProvider.UnsetUserGroupPermission(applicationId, groupId, roleId, currentUserId);
        }

        public static List<UserGroup> get_user_groups(Guid applicationId, List<Guid> groupIds)
        {
            List<UserGroup> groups = new List<UserGroup>();
            DataProvider.GetUserGroups(applicationId, ref groups, groupIds);
            return groups;
        }

        public static List<UserGroup> get_user_groups(Guid applicationId)
        {
            return UsersController.get_user_groups(applicationId, new List<Guid>());
        }

        public static UserGroup get_user_group(Guid applicationId, Guid groupId)
        {
            return UsersController.get_user_groups(applicationId, new List<Guid>() { groupId }).FirstOrDefault();
        }

        public static List<User> get_user_group_members(Guid applicationId, Guid groupId)
        {
            List<User> lst = new List<User>();
            DataProvider.GetUserGroupMembers(applicationId, ref lst, groupId);
            return lst;
        }

        public static List<AccessRole> get_user_group_access_roles(Guid applicationId, Guid groupId)
        {
            List<AccessRole> lst = new List<AccessRole>();
            DataProvider.GetUserGroupAccessRoles(applicationId, ref lst, groupId);
            return lst;
        }

        public static List<AccessRoleName> check_user_group_permissions(Guid applicationId, Guid userId, 
            List<AccessRoleName> permissions)
        {
            return DataProvider.CheckUserGroupPermissions(applicationId, userId, permissions);
        }

        public static List<AccessRole> get_access_roles(Guid applicationId)
        {
            List<AccessRole> roles = new List<AccessRole>();
            DataProvider.GetAccessRoles(applicationId, ref roles);
            return roles;
        }

        /* end of User Groups */
    }
}