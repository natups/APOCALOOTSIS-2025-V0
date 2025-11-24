mergeInto(LibraryManager.library, {
    // -------------------------------------------------------------------
    // Funciones de Login y Register (Llamadas desde C#)
    // -------------------------------------------------------------------

    RegisterUser: function (emailPtr, passwordPtr, objectNamePtr, successCallbackPtr, errorCallbackPtr) {
        var email = UTF8ToString(emailPtr);
        var password = UTF8ToString(passwordPtr);
        var objectName = UTF8ToString(objectNamePtr);
        var successCallback = UTF8ToString(successCallbackPtr);
        var errorCallback = UTF8ToString(errorCallbackPtr);
        
        // Verifica si el servicio de Firebase está disponible
        if (typeof firebaseAuthService === 'undefined') {
            var errorMessage = "Error JS: firebaseAuthService is not defined in the HTML/JS setup.";
            // Llama a la función de error en C#
            unityInstance.SendMessage(objectName, errorCallback, errorMessage);
            return;
        }

        firebaseAuthService.register(email, password)
            .then(user => {
                var userData = JSON.stringify({
                    uid: user.uid,
                    email: user.email,
                    displayName: user.displayName || ""
                });
                // Llama a la función de éxito en C#
                unityInstance.SendMessage(objectName, successCallback, userData);
            })
            .catch(error => {
                var errorMessage = "Firebase Register Error: " + error.message;
                // Llama a la función de error en C#
                unityInstance.SendMessage(objectName, errorCallback, errorMessage);
            });
    },

    SignInUser: function (emailPtr, passwordPtr, objectNamePtr, successCallbackPtr, errorCallbackPtr) {
        var email = UTF8ToString(emailPtr);
        var password = UTF8ToString(passwordPtr);
        var objectName = UTF8ToString(objectNamePtr);
        var successCallback = UTF8ToString(successCallbackPtr);
        var errorCallback = UTF8ToString(errorCallbackPtr);
        
        // Verifica si el servicio de Firebase está disponible
        if (typeof firebaseAuthService === 'undefined') {
            var errorMessage = "Error JS: firebaseAuthService is not defined in the HTML/JS setup.";
            unityInstance.SendMessage(objectName, errorCallback, errorMessage);
            return;
        }

        firebaseAuthService.signIn(email, password)
            .then(user => {
                var userData = JSON.stringify({
                    uid: user.uid,
                    email: user.email,
                    displayName: user.displayName || ""
                });
                // Llama a la función de éxito en C#
                unityInstance.SendMessage(objectName, successCallback, userData);
            })
            .catch(error => {
                var errorMessage = "Firebase Sign In Error: " + error.message;
                unityInstance.SendMessage(objectName, errorCallback, errorMessage);
            });
    },

    // -------------------------------------------------------------------
    // NUEVA FUNCIÓN: Cerrar Sesión (SignOutUser)
    // La implementación debe estar aquí para que el linker la encuentre.
    // -------------------------------------------------------------------
    SignOutUser: function () {
        // Asumimos que la variable 'auth' de Firebase se ha asignado al objeto global 'window' en index.html
        var authInstance = window.auth || firebase.auth(); 

        if (authInstance) {
            console.log("JSLIB: Ejecutando SignOutUser() de Firebase...");
            
            authInstance.signOut()
                .then(function() {
                    console.log("JSLIB: Sesión de Firebase cerrada exitosamente.");
                })
                .catch(function(error) {
                    console.error("JSLIB Error al cerrar sesión:", error.message);
                });
        } else {
            console.error("JSLIB Error: El objeto Auth de Firebase no está disponible.");
        }
    }
});