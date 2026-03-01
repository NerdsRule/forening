/**
 * Web Authentication API wrapper module for credential creation and assertion
 * @namespace orgWebAuth
 */

/**
 * Converts an ArrayBuffer to a Base64URL encoded string
 * @param {ArrayBuffer} buffer - The buffer to encode
 * @returns {string} Base64URL encoded string
 * @private
 */

/**
 * Converts a Base64URL encoded string back to a Uint8Array
 * @param {string} value - The Base64URL encoded string
 * @returns {Uint8Array} Decoded byte array
 * @private
 */

/**
 * Checks if the browser supports WebAuthn API
 * @returns {boolean} True if WebAuthn is supported, false otherwise
 */

/**
 * Creates a new WebAuthn credential (registration)
 * @param {Object} options - Configuration options for credential creation
 * @param {string} options.challenge - Base64URL encoded challenge from server
 * @param {string} options.rpName - Relying party name
 * @param {string} options.rpId - Relying party ID (domain)
 * @param {string} options.userId - Base64URL encoded user ID
 * @param {string} options.userName - User account name
 * @param {string} options.displayName - User display name
 * @param {number} [options.timeoutMs=60000] - Timeout in milliseconds
 * @returns {Promise<Object|null>} Credential object with credentialId, clientDataJson, publicKeySpki, and publicKeyAlgorithm, or null if creation failed
 * @async
 */

/**
 * Gets an assertion from an existing credential (authentication)
 * @param {Object} options - Configuration options for assertion retrieval
 * @param {string} options.challenge - Base64URL encoded challenge from server
 * @param {string} options.rpId - Relying party ID (domain)
 * @param {string[]} [options.allowCredentialIds=[]] - Array of Base64URL encoded credential IDs to allow
 * @param {number} [options.timeoutMs=60000] - Timeout in milliseconds
 * @returns {Promise<Object|null>} Assertion object with credentialId, clientDataJson, authenticatorData, signature, and userHandle, or null if assertion failed
 * @async
 */
window.orgWebAuth = (() => {
    // Helper functions for Base64URL encoding and decoding
    const toBase64Url = (buffer) => {
        const bytes = new Uint8Array(buffer);
        let binary = "";
        for (let i = 0; i < bytes.byteLength; i++) {
            binary += String.fromCharCode(bytes[i]);
        }
        return btoa(binary).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/g, "");
    };

    // Converts a Base64URL encoded string back to a Uint8Array
    const fromBase64Url = (value) => {
        if (!value) {
            return new Uint8Array();
        }
        const base64 = value.replace(/-/g, "+").replace(/_/g, "/");
        const padded = base64 + "=".repeat((4 - (base64.length % 4)) % 4);
        const binary = atob(padded);
        const bytes = new Uint8Array(binary.length);
        for (let i = 0; i < binary.length; i++) {
            bytes[i] = binary.charCodeAt(i);
        }
        return bytes;
    };

    // Checks if the browser supports WebAuthn API
    const hasWebAuthn = () => {
        return typeof window !== "undefined"
            && !!window.PublicKeyCredential
            && !!navigator.credentials;
    };

    // Creates a new WebAuthn credential (registration)
    const createCredential = async (options) => {
        const publicKey = {
            challenge: fromBase64Url(options.challenge),
            rp: {
                name: options.rpName,
                id: options.rpId
            },
            user: {
                id: fromBase64Url(options.userId),
                name: options.userName,
                displayName: options.displayName
            },
            pubKeyCredParams: [
                { type: "public-key", alg: -7 },
                { type: "public-key", alg: -257 }
            ],
            timeout: options.timeoutMs ?? 60000,
            authenticatorSelection: {
                residentKey: "preferred",
                userVerification: "preferred"
            },
            attestation: "none"
        };

        const credential = await navigator.credentials.create({ publicKey });
        if (!credential) {
            return null;
        }

        const response = credential.response;
        const credentialId = toBase64Url(credential.rawId);
        const clientDataJson = toBase64Url(response.clientDataJSON);

        let publicKeySpki = "";
        let publicKeyAlgorithm = -7;
        if (typeof response.getPublicKey === "function") {
            const pk = response.getPublicKey();
            if (pk) {
                publicKeySpki = toBase64Url(pk);
            }
        }
        if (typeof response.getPublicKeyAlgorithm === "function") {
            publicKeyAlgorithm = response.getPublicKeyAlgorithm();
        }

        return {
            credentialId,
            clientDataJson,
            publicKeySpki,
            publicKeyAlgorithm
        };
    };

    /**
     * Gets an assertion from an existing credential (authentication)
     * @param {Object} options - Configuration options for assertion retrieval
     * @param {string} options.challenge - Base64URL encoded challenge from server
     * @param {string} options.rpId - Relying party ID (domain)
     * @param {string[]} [options.allowCredentialIds=[]] - Array of Base64URL encoded credential IDs to allow
     * @param {number} [options.timeoutMs=60000] - Timeout in milliseconds
     * @returns {Promise<Object|null>} Assertion object with credentialId, clientDataJson, authenticatorData, signature, and userHandle, or null if assertion failed
     * @async
     */
    const getAssertion = async (options) => {
        const publicKey = {
            challenge: fromBase64Url(options.challenge),
            timeout: options.timeoutMs ?? 60000,
            rpId: options.rpId,
            allowCredentials: (options.allowCredentialIds ?? []).map((id) => ({
                id: fromBase64Url(id),
                type: "public-key"
            })),
            userVerification: "preferred"
        };

        const assertion = await navigator.credentials.get({ publicKey });
        if (!assertion) {
            return null;
        }

        const response = assertion.response;
        return {
            credentialId: toBase64Url(assertion.rawId),
            clientDataJson: toBase64Url(response.clientDataJSON),
            authenticatorData: toBase64Url(response.authenticatorData),
            signature: toBase64Url(response.signature),
            userHandle: response.userHandle ? toBase64Url(response.userHandle) : null
        };
    };

    // Expose the public API
    return {
        hasWebAuthn,
        createCredential,
        getAssertion
    };
})();
