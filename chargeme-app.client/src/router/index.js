import { createRouter, createWebHistory } from 'vue-router'
import Home from '../views/Home.vue'
import ScanQRcode from '../views/ScanQRcode.vue'
import Station from '../views/Station.vue'
import Login from '../views/Login.vue'
import Register from '../views/Register.vue'
import UserProfile from '../views/UserProfile.vue'
import Transaction from '../views/Transaction.vue'
import ChargeStatus from '../views/ChargeStatus.vue'
import ChargeFinish from '../views/ChargeFinish.vue'
import QRCode from '../views/QRCode.vue';
const router = createRouter({
    history: createWebHistory(import.meta.env.VITE_BASE_URL),
    routes: [
        {
            path: '/',
            name: 'home',
            component: Home
        },
        {
            path: '/scanqr',
            name: 'scanqr',
            component: ScanQRcode,
        },
        {
            path: '/:id',
            name: 'station',
            component: Station,
            props: route => ({
                id: route.params.id
            })
        },
        //{
        //    path: '/chargestation',
        //    name: 'chargestation',
        //    component: Station,
        //    props: route => ({
        //        chargedata: route.query
        //    })
        //},
        {
            path: '/login',
            name: 'login',
            component: Login,
            props: route => ({
                fid: route.query
            })
        },
        {
            path: '/register',
            name: 'register',
            component: Register,
            props: route => ({
                fid: route.query
            })
        },
        {
            path: '/profile',
            name: 'profile',
            component: UserProfile
        },
        {
            path: '/trans',
            name: 'trans',
            component: Transaction
        },
        {
            path: '/status',
            name: 'status',
            component: ChargeStatus
        },
        {
            path: '/finish',
            name: 'finish',
            component: ChargeFinish
        },
        {
            path: '/qrcode',
            name: 'qrcode',
            component: QRCode
        },
    ]
})
export default router