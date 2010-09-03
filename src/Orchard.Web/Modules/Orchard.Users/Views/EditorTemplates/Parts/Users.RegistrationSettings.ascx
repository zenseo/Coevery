<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<RegistrationSettingsPartRecord>" %>
<%@ Import Namespace="Orchard.Users.Models"%>
<fieldset>
    <legend><%: T("Users registration")%></legend>
    <div>
        <%: Html.EditorFor(m => m.UsersCanRegister) %>
        <label class="forcheckbox" for="<%: Html.FieldIdFor( m => m.UsersCanRegister) %>"><%: T("Users can create new accounts on the site")%></label>
        <%: Html.ValidationMessage("UsersCanRegister", "*")%>
    </div>
    <div>
        <%: Html.EditorFor(m => m.UsersMustValidateEmail)%>
        <label class="forcheckbox" for="<%: Html.FieldIdFor( m => m.UsersMustValidateEmail) %>"><%: T("Users must justify their email address")%></label>
        <%: Html.ValidationMessage("UsersMustValidateEmail", "*")%>
    </div>
    <div>
        <%: Html.EditorFor(m => m.UsersAreModerated)%>
        <label class="forcheckbox" for="<%: Html.FieldIdFor( m => m.UsersAreModerated) %>"><%: T("Users must be approved before they can log in")%></label>
        <%: Html.ValidationMessage("UsersAreModerated", "*")%>
    </div>    
    <div data-controllerid="<%:Html.FieldIdFor(m => m.UsersAreModerated) %>">
        <%: Html.EditorFor(m => m.NotifyModeration)%>
        <label class="forcheckbox" for="<%: Html.FieldIdFor( m => m.NotifyModeration) %>"><%: T("Send a notification when a user needs moderation")%></label>
        <%: Html.ValidationMessage("NotifyModeration", "*")%>
    </div>
</fieldset>