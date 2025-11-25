mergeInto(LibraryManager.library, {
    
    // Función de Registro
    RegisterUser: function (email, password, gameObjectName, successCallback, failureCallback) {
        // --- CONVERSIÓN DE PUNTEROS (Mandatorio en WebGL) ---
        var emailStr = UTF8ToString(email);
        var passwordStr = UTF8ToString(password);
        var goName = UTF8ToString(gameObjectName);
        var successFunc = UTF8ToString(successCallback);
        var failureFunc = UTF8ToString(failureCallback);
        // -------------------------

        // Asegúrate de que window.firebaseAuthService esté disponible
        if (typeof window.firebaseAuthService === 'undefined') {
            SendMessage(goName, failureFunc, "Error JS: firebaseAuthService is not defined in the HTML/JS setup.");
            return;
        }

        // Llama a la función de registro
        window.firebaseAuthService.register(emailStr, passwordStr)
            .then(function(user) {
                // Éxito: Devuelve el UID del usuario a Unity
                if (user && user.uid) {
                    SendMessage(goName, successFunc, user.uid);
                } else {
                    SendMessage(goName, failureFunc, "Unknown registration error.");
                }
            })
            .catch(function(error) {
                // Error: Devuelve el mensaje de error a Unity
                SendMessage(goName, failureFunc, error.message);
            });
    },

    // Función de Inicio de Sesión
    SignInUser: function (email, password, gameObjectName, successCallback, failureCallback) {
        // --- CONVERSIÓN DE PUNTEROS ---
        var emailStr = UTF8ToString(email);
        var passwordStr = UTF8ToString(password);
        var goName = UTF8ToString(gameObjectName);
        var successFunc = UTF8ToString(successCallback);
        var failureFunc = UTF8ToString(failureCallback);
        // -------------------------

        // Asegúrate de que window.firebaseAuthService esté disponible
        if (typeof window.firebaseAuthService === 'undefined') {
            SendMessage(goName, failureFunc, "Error JS: firebaseAuthService is not defined in the HTML/JS setup.");
            return;
        }

        // Llama a la función de inicio de sesión
        window.firebaseAuthService.signIn(emailStr, passwordStr)
            .then(function(user) {
                // Éxito: Devuelve el UID del usuario a Unity
                if (user && user.uid) {
                    SendMessage(goName, successFunc, user.uid);
                } else {
                    SendMessage(goName, failureFunc, "Unknown sign-in error.");
                }
            })
            .catch(function(error) {
                // Error: Devuelve el mensaje de error a Unity
                SendMessage(goName, failureFunc, error.message);
            });
    },

    // NUEVA FUNCIÓN: Cerrar Sesión
    SignOutUser: function (gameObjectName, successCallback, failureCallback) {
        // --- CONVERSIÓN DE PUNTEROS ---
        var goName = UTF8ToString(gameObjectName);
        var successFunc = UTF8ToString(successCallback);
        var failureFunc = UTF8ToString(failureCallback);
        // -------------------------

        // Asegúrate de que window.firebaseAuthService esté disponible
        if (typeof window.firebaseAuthService === 'undefined') {
            SendMessage(goName, failureFunc, "Error JS: firebaseAuthService is not defined in the HTML/JS setup.");
            return;
        }

        // Llama a la función de cierre de sesión
        window.firebaseAuthService.signOut()
            .then(function() {
                // Éxito: El cierre de sesión no devuelve datos, solo confirmación
                SendMessage(goName, successFunc, "Logout successful.");
            })
            .catch(function(error) {
                // Error: Devuelve el mensaje de error a Unity
                SendMessage(goName, failureFunc, error.message);
            });
    }
});