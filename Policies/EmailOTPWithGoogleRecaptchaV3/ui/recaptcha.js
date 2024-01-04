/* eslint-disable */
const createAccountButton = document.getElementById('continue')
let recaptchaLoaded = false
let recaptchaTimer;

function showIntroState() {
  // Mark which page we are on to contrain some of our styles
  document.body.classList.add('state--intro')

  // Insert a new button that will be used in place of the default continue
  // button.  This will be used to trigger reCAPTCHA.
  const signInButton = document.createElement('button')
  signInButton.setAttribute('id', 'sign-in')
  signInButton.setAttribute('type', 'button')
  //signInButton.setAttribute('disabled', 'disabled')
  signInButton.innerHTML = 'Continue'
  document.querySelector('.buttons').appendChild(signInButton)

  // Hide the original sign-in button
  const continueButton = document.getElementById('next')
  continueButton.classList.add('hidden')

  // Hide the password field's parent.  We are only using this to store the
  // CAPTCHA token and it should not be user-editable.
  const passwordField = document.getElementById('password')
  if(passwordField) {
    passwordField.parentNode.classList.add('hidden')
  }

  // Remove the password status on the password field, remove aria values, etc.
  passwordField.setAttribute('type', 'hidden')
  passwordField.setAttribute('aria-hidden', 'true')
  passwordField.setAttribute('autocomplete', 'off')

  if(window.grecaptcha !== undefined) {
    setupRecaptcha(signInButton)
  } else {
    recaptchaTimer = setInterval(function() {
      if(window.grecaptcha !== undefined) {
        setupRecaptcha(signInButton)
        clearInterval(recaptchaTimer)
      }
    }, 100)
  }
}

function showLoginState() {
  // Disable the "Sign in" button by default.  It should be re-enabled
  // when the email and password fields are filled in.
  const signInButton = document.getElementById('next')
  //signInButton.setAttribute('disabled', 'disabled')
}

function setupRecaptcha (signInButton) {
  recaptchaLoaded = true
  const recaptchaDiv = document.createElement('div')
  recaptchaDiv.classList.add('g-recaptcha')
  document.forms[0].appendChild(recaptchaDiv)
  grecaptcha.ready(function() {
    grecaptcha.render(recaptchaDiv, {
      "sitekey": window.RECAPTCHA_KEY,
      "callback": onSubmit,
      "size": "invisible"
    });
  });

  // When clicking our synthetic sign-in button, trigger reCAPTCHA
  signInButton.onclick = function(e) {
    grecaptcha.execute()
  }
}

showIntroState()
showLoginState()
