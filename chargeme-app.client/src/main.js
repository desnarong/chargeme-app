/* eslint-disable no-undef */
import './assets/main.css'
import { library, dom } from "@fortawesome/fontawesome-svg-core";
import { fas } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import Vuex from 'vuex';

library.add(fas)
dom.watch();

import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
//import i18n from "./i18n"
import axios from 'axios'
import VueAxios from "vue-axios"
import store from './services/store'; // นำเข้า store ที่เราสร้างไว้

const app = createApp(App)

// Axios Plugin
app.use(Vuex)

app.use(VueAxios, axios)
app.use(router)
//app.use(i18n)
app.use(store) // เพิ่ม store ให้แอป
app.component("font-awesome-icon", FontAwesomeIcon)
app.mount('#app')
