using GreenSync.Lib.Services;

namespace GreenSync_app.Models;

public class ProfileViewModel
{
    public User User { get; set; } = new();
    public int TotalReports { get; set; }
    public decimal EcoCreditsBalance { get; set; }
    public DateTime MemberSince { get; set; }
}

public class EditProfileViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class ChangePasswordViewModel
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
