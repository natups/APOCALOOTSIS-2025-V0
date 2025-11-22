mergeInto(LibraryManager.library, {

  // Funcion para REGISTRO de nuevo usuario
  RegisterUser: function (email, password, gameObject, successCallback, failureCallback) {
    
    // Convierte los punteros de C# a cadenas de JavaScript
    var jsEmail = Pointer_stringify(email);
    var jsPassword = Pointer_stringify(password);
    var jsObject = Pointer_stringify(gameObject);
    var jsSuccess = Pointer_stringify(successCallback);
    var jsFailure = Pointer_stringify(failureCallback);

    // Llama a Firebase Authentication para crear el usuario
    firebase.auth().createUserWithEmailAndPassword(jsEmail, jsPassword)
    .then((userCredential) => {
      // ÉXITO: Devuelve el ID del usuario (UID)
      unityInstance.SendMessage(jsObject, jsSuccess, userCredential.user.uid);
    })
    .catch((error) => {
      // ERROR: Devuelve el mensaje de error de Firebase
      unityInstance.SendMessage(jsObject, jsFailure, error.message);
    });
  }, // <--- ¡Asegúrate que esta coma exista si tienes más funciones abajo!

  // Funcion para INICIO de SESIÓN
  SignInUser: function (email, password, gameObject, successCallback, failureCallback) {
    
    var jsEmail = Pointer_stringify(email);
    var jsPassword = Pointer_stringify(password);
    var jsObject = Pointer_stringify(gameObject);
    var jsSuccess = Pointer_stringify(successCallback);
    var jsFailure = Pointer_stringify(failureCallback);

    // Llama a Firebase Authentication para iniciar sesión
    firebase.auth().signInWithEmailAndPassword(jsEmail, jsPassword)
    .then((userCredential) => {
      // ÉXITO: Devuelve el ID del usuario (UID)
      unityInstance.SendMessage(jsObject, jsSuccess, userCredential.user.uid);
    })
    .catch((error) => {
      // ERROR: Devuelve el mensaje de error de Firebase
      unityInstance.SendMessage(jsObject, jsFailure, error.message);
    });
  } 

});