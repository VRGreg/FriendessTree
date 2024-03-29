﻿namespace backend.Models
{
    public class UserProfile
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public UserProfile() { }
        public UserProfile(AppUser user)
        {
            Id = user.Id != null ? user.Id : "";
            FirstName = user.FirstName != null ? user.FirstName : "";
            LastName = user.LastName != null ? user.LastName : "";
            Phone = user.PhoneNumber != null && user.PhoneNumberConfirmed ? user.PhoneNumber : "";
            Email = user.Email != null ? user.Email : "";
        }
    }
}
