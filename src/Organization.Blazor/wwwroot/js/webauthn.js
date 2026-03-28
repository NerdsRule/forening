(function () {
    function decodeBase64Url(value) {
        const base64 = value.replace(/-/g, '+').replace(/_/g, '/');
        const padded = base64 + '='.repeat((4 - (base64.length % 4)) % 4);
        const raw = atob(padded);
        const bytes = new Uint8Array(raw.length);
        for (let i = 0; i < raw.length; i++) {
            bytes[i] = raw.charCodeAt(i);
        }
        return bytes.buffer;
    }

    function encodeBase64Url(buffer) {
        const bytes = new Uint8Array(buffer);
        let binary = '';
        for (let i = 0; i < bytes.byteLength; i++) {
            binary += String.fromCharCode(bytes[i]);
        }
        return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/g, '');
    }

    function normalizeCreationOptions(options) {
        const normalized = structuredClone(options);
        normalized.challenge = decodeBase64Url(normalized.challenge);
        normalized.user.id = decodeBase64Url(normalized.user.id);
        if (Array.isArray(normalized.excludeCredentials)) {
            normalized.excludeCredentials = normalized.excludeCredentials.map(c => ({
                ...c,
                id: decodeBase64Url(c.id)
            }));
        }
        return normalized;
    }

    function normalizeRequestOptions(options) {
        const normalized = structuredClone(options);
        normalized.challenge = decodeBase64Url(normalized.challenge);
        if (Array.isArray(normalized.allowCredentials)) {
            normalized.allowCredentials = normalized.allowCredentials.map(c => ({
                ...c,
                id: decodeBase64Url(c.id)
            }));
        }
        return normalized;
    }

    function serializeRegistrationCredential(credential) {
        const response = credential.response;
        const transports = typeof response.getTransports === 'function'
            ? response.getTransports()
            : [];

        return {
            id: credential.id,
            rawId: encodeBase64Url(credential.rawId),
            type: credential.type,
            authenticatorAttachment: credential.authenticatorAttachment ?? null,
            clientExtensionResults: credential.getClientExtensionResults(),
            response: {
                attestationObject: encodeBase64Url(response.attestationObject),
                clientDataJSON: encodeBase64Url(response.clientDataJSON),
                transports: transports
            }
        };
    }

    function serializeAssertionCredential(assertion) {
        const response = assertion.response;

        return {
            id: assertion.id,
            rawId: encodeBase64Url(assertion.rawId),
            type: assertion.type,
            authenticatorAttachment: assertion.authenticatorAttachment ?? null,
            clientExtensionResults: assertion.getClientExtensionResults(),
            response: {
                authenticatorData: encodeBase64Url(response.authenticatorData),
                clientDataJSON: encodeBase64Url(response.clientDataJSON),
                signature: encodeBase64Url(response.signature),
                userHandle: response.userHandle ? encodeBase64Url(response.userHandle) : null
            }
        };
    }

    window.organizationWebAuthn = {
        async createCredentialJson(options) {
            if (!window.PublicKeyCredential || !navigator.credentials) {
                throw new Error('WebAuthn is not supported in this browser.');
            }
            const credential = await navigator.credentials.create({ publicKey: normalizeCreationOptions(options) });
            if (!credential) {
                throw new Error('No passkey credential returned by browser.');
            }
            return JSON.stringify(serializeRegistrationCredential(credential));
        },

        async getAssertionJson(options) {
            if (!window.PublicKeyCredential || !navigator.credentials) {
                throw new Error('WebAuthn is not supported in this browser.');
            }
            const assertion = await navigator.credentials.get({ publicKey: normalizeRequestOptions(options) });
            if (!assertion) {
                throw new Error('No passkey assertion returned by browser.');
            }
            return JSON.stringify(serializeAssertionCredential(assertion));
        }
    };

    window.organizationApp = {
        async hardRefresh(targetUrl) {
            try {
                if ('serviceWorker' in navigator) {
                    const registrations = await navigator.serviceWorker.getRegistrations();
                    await Promise.all(registrations.map(registration => registration.unregister()));
                }

                if ('caches' in window) {
                    const cacheKeys = await caches.keys();
                    await Promise.all(cacheKeys.map(cacheKey => caches.delete(cacheKey)));
                }

                window.localStorage.clear();
                window.sessionStorage.clear();
            } finally {
                window.location.replace(targetUrl || window.location.href);
            }
        }
    };
})();
