﻿namespace CRM_mvc.Models.Views.Account;

public class ResetPasswordViewModel
{
    public string Email { get; set; }
    public string Token { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}