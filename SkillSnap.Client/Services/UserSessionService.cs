namespace SkillSnap.Client.Services;

public class UserSessionService
{
    public string? UserId { get; private set; }
    public string? UserName { get; private set; }
    public string? Email { get; private set; }
    public string? Role { get; private set; }
    public int? SelectedProfileId { get; private set; }
    public int? CurrentProjectId { get; private set; }
    public string? CurrentProjectTitle { get; private set; }
    public int? CurrentEditingProfileId { get; private set; }
    public string? CurrentEditingProfileName { get; private set; }

    public bool HasUser => !string.IsNullOrWhiteSpace(UserId);

    public event Action? StateChanged;

    public void SetUser(string? userId, string? userName, string? email, string? role)
    {
        UserId = userId;
        UserName = userName;
        Email = email;
        Role = string.IsNullOrWhiteSpace(role) ? "User" : role;
        NotifyStateChanged();
    }

    public void SetSelectedProfile(int profileId)
    {
        SelectedProfileId = profileId;
        NotifyStateChanged();
    }

    public void SetCurrentProject(int? projectId, string? projectTitle)
    {
        CurrentProjectId = projectId;
        CurrentProjectTitle = projectTitle;
        NotifyStateChanged();
    }

    public void SetCurrentEditingProfile(int? profileId, string? profileName)
    {
        CurrentEditingProfileId = profileId;
        CurrentEditingProfileName = profileName;
        NotifyStateChanged();
    }

    public void Clear()
    {
        UserId = null;
        UserName = null;
        Email = null;
        Role = null;
        SelectedProfileId = null;
        CurrentProjectId = null;
        CurrentProjectTitle = null;
        CurrentEditingProfileId = null;
        CurrentEditingProfileName = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
