using System;

namespace Organization.Shared.Identity;

/// <summary>
/// Role claim from identity endpoint to establish claims.
/// </summary>
public class RoleClaim
    {
        /// <summary>
        /// The claim issuer.
        /// </summary>
        public string? Issuer { get; set; }

        /// <summary>
        /// The original issuer.
        /// </summary>
        public string? OriginalIssuer { get; set; }

        /// <summary>
        /// Claim type.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Claim value.
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// The value type.
        /// </summary>
        public string? ValueType { get; set; }
    }