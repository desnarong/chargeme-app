<template>
  <div class="container-fluid">
      <div class="row">
          <div class="col-12 p-0">
              <div class="login-card login-dark">
                  <div>
                      <div> <router-link class="logo" to="/"><img class="img-fluid for-light"
                                  src="@/assets/images/logo/logo.png" alt="looginpage"><img class="img-fluid for-dark"
                                  src="@/assets/images/logo/logo_dark.png" alt="looginpage"></router-link></div>
                      <div class="login-main">
                          <form class="theme-form">
                              <h2>Sign in to account</h2>
                              <p>Enter your email & password to login</p>
                              <div class="form-group">
                                  <label class="col-form-label">Email Address</label>
                                  <input class="form-control" type="email" required placeholder="Test@gmail.com">
                              </div>
                              <div class="form-group">
                                  <label class="col-form-label">Password</label>
                                  <div class="form-input position-relative">
                                      <input class="form-control" :type="type" name="login[password]"
                                          placeholder="*********">
                                      <div class="show-hide"><span :class="type == 'password' ? 'show' : ''"
                                              @click="showPassword"> </span></div>
                                  </div>
                              </div>
                              <div class="form-group mb-0">
                                  <div class="checkbox p-0">
                                      <input id="checkbox1" type="checkbox">
                                      <label class="text-muted" for="checkbox1">Remember password</label>
                                  </div><a class="link"><router-link to="/authentication/forget_password">Forgot
                                          password?</router-link></a>
                                  <div class="text-end mt-3">
                                      <button class="btn btn-primary btn-block w-100" type="submit">Sign in</button>
                                  </div>
                              </div>
                              <h6 class="text-muted mt-4 or">Or Sign in with</h6>
                              <div class="social mt-4">
                                  <div class="btn-showcase"><a class="btn btn-light" href="https://www.linkedin.com/login"
                                          target="_blank"><vue-feather class="txt-linkedin" type="linkedin"></vue-feather>
                                          LinkedIn
                                      </a><a class="btn btn-light" href="https://twitter.com/login?lang=en"
                                          target="_blank"><vue-feather class="txt-twitter"
                                              type="twitter"></vue-feather>twitter</a><a class="btn btn-light"
                                          href="https://www.facebook.com/" target="_blank"><vue-feather class="txt-fb"
                                              type="facebook"></vue-feather>facebook</a></div>
                              </div>
                              <p class="mt-4 mb-0 text-center">Don't have account?<router-link class="ms-2"
                                      to="/auth/register">Create Account</router-link></p>
                          </form>
                      </div>
                  </div>
              </div>
          </div>
      </div>

  </div>
</template>
<script lang="ts" setup>
import { ref } from 'vue'
const type = ref<string>('password')
function showPassword() {
  if (type.value === 'password') {
      type.value = 'text';
  } else {
      type.value = 'password';
  }
}
</script>
