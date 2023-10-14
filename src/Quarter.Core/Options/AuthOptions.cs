namespace Quarter.Core.Options;

public class AuthOptions
{
    /// <summary>
    /// Whether or not registrations are open to anyone. If false (default) users can only be added via the
    /// admin backend (and the admin is added via the InitialUser option in appsettings.
    /// </summary>
    public bool OpenRegistration { get; set; } = false;
}