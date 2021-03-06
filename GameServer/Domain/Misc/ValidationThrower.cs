﻿using System;
using System.Linq;

namespace Domain.Misc
{
    /// <summary>
    /// Template for throwing in relation to user validation
    /// </summary>
    public abstract class BaseThrow
    {
        /// <summary>
        /// The recipe
        /// </summary>
        /// <param name="value">the setters value</param>
        public void Validate(string value)
        {
            ThrowArgumentIf(string.IsNullOrEmpty(value), "value cannot be empty!");
            ValidateMore(value);
        }
        /// <summary>
        /// this method is the one to overide in derived classes, use this to specify specific throws
        /// </summary>
        /// <param name="value">value from setter</param>
        protected abstract void ValidateMore(string value);
        /// <summary>
        /// A helper method
        /// </summary>
        /// <param name="condition">when to throw a message</param>
        /// <param name="message">The message to throw</param>
        protected void ThrowArgumentIf(bool condition, string message)
        {
            if (condition)
                throw new ArgumentException(message);
        }
    }

    public class UsernameThrow : BaseThrow
    {

        protected override void ValidateMore(string value)
        {
            ThrowArgumentIf(value.Length < 8, "value cannot be less than 8 characters");
            ThrowArgumentIf(value.Length > 20, "value cannot be more than 20 characters");
            ThrowArgumentIf(value.Any(x => !Char.IsLetterOrDigit(x)), "value only be letters from a to z or numbers");
        }

    }

    public class PasswordThrow : BaseThrow
    {
        protected override void ValidateMore(string value)
        {
            ThrowArgumentIf(value.Length < 8, "value cannot be less than 8 characters");
        }
    }

    public class EmailThrow : BaseThrow
    {
        protected override void ValidateMore(string value)
        {
            ThrowArgumentIf(value.Length < 5, "value cannot be less than 5 characters");
            ThrowArgumentIf(!(value.Contains("@") && value.Contains(".")), "Not a valid Email");
        }
    }

    public class NameThrow : BaseThrow
    {
        protected override void ValidateMore(string value)
        {
            ThrowArgumentIf((value.Contains(".") || value.Length < 2), "Initials is not allowed");
            ThrowArgumentIf(value.Any(x => !Char.IsLetter(x)), "Name can only contain letters");
        }
    }

}
