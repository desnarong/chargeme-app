import { createStore } from 'vuex';

const store = createStore({
    state: {
        token: localStorage.getItem('token') || '',
        fid: (() => {
            try {
                return JSON.parse(localStorage.getItem('fid')) || '';
            } catch (error) {
                console.error('Error parsing fid:', error);
                return '';
            }
        })(),
        user: (() => {
            try {
                return JSON.parse(localStorage.getItem('user')) || {};
            } catch (error) {
                console.error('Error parsing user:', error);
                return {};
            }
        })(),
        chargedata: localStorage.getItem('chargedata') ? JSON.parse(localStorage.getItem('chargedata')) : {},
        chargestatusdata: localStorage.getItem('chargestatusdata') ? JSON.parse(localStorage.getItem('chargestatusdata')) : {},
        paydata: (() => {
            try {
                return JSON.parse(localStorage.getItem('paydata')) || {};
            } catch (error) {
                console.error('Error parsing paydata:', error);
                return {};
            }
        })(),
        transdata: (() => {
            try {
                return JSON.parse(localStorage.getItem('transdata')) || {};
            } catch (error) {
                console.error('Error parsing transdata:', error);
                return {};
            }
        })(),
        translations: localStorage.getItem('translations') ? JSON.parse(localStorage.getItem('translations')) : {},
        currentlanguage: localStorage.getItem('currentlanguage') || 'en', // Default เป็น 'en'
    },
    mutations: {
        setToken(state, token) {
            state.token = token;
            localStorage.setItem('token', token);
        },
        clearToken(state) {
            state.token = '';
            localStorage.setItem('token', '');
        },
        setFid(state, fid) {
            state.fid = fid;
            localStorage.setItem('fid', JSON.stringify(fid));
        },
        setUser(state, user) {
            state.user = user;
            localStorage.setItem('user', JSON.stringify(user));
        },
        clearUser(state) {
            state.user = {};
            localStorage.setItem('user', '');
        },
        setChargedata(state, chargedata) {
            state.chargedata = chargedata;
            localStorage.setItem('chargedata', JSON.stringify(chargedata));
        },
        setPaydata(state, paydata) {
            state.paydata = paydata;
            localStorage.setItem('paydata', JSON.stringify(paydata));
        },
        clearPaydata(state) {
            state.paydata = {};
            localStorage.setItem('paydata', '');
        },
        setTransdata(state, transdata) {
            state.transdata = transdata;
            localStorage.setItem('transdata', JSON.stringify(transdata));
        },
        clearTransdata(state) {
            state.transdata = {};
            localStorage.setItem('transdata', '');
        },
        setTranslations(state, translations) {
            state.translations = translations;
            localStorage.setItem('translations', JSON.stringify(translations));
        },
        setCurrentlanguage(state, currentlanguage) {
            state.currentlanguage = String(currentlanguage); // แปลงเป็น string เสมอ
            localStorage.setItem('currentlanguage', state.currentlanguage);
        },
        setChargestatusdata(state, chargestatusdata) {
            state.chargestatusdata = chargestatusdata;
            localStorage.setItem('chargestatusdata', JSON.stringify(chargestatusdata));
        },
        clearChargestatusdata(state) {
            state.chargestatusdata = {};
            localStorage.setItem('chargestatusdata', '');
        },
    },
    actions: {
        saveToken({ commit }, token) {
            commit('setToken', token);
        },
        removeToken({ commit }) {
            commit('clearToken');
        },
        updateFid({ commit }, fid) {
            commit('setFid', fid);
        },
        updateUser({ commit }, user) {
            commit('setUser', user);
        },
        removeUser({ commit }) {
            commit('clearUser');
        },
        updatePaydata({ commit }, paydata) {
            commit('setPaydata', paydata);
        },
        removePaydata({ commit }) {
            commit('clearPaydata');
        },
        updateTransdata({ commit }, transdata) {
            commit('setTransdata', transdata);
        },
        removeTransdata({ commit }) {
            commit('clearTransdata');
        },
        updateChargedata({ commit }, chargedata) {
            commit('setChargedata', chargedata);
        },
        updateTranslations({ commit }, translations) {
            commit('setTranslations', translations);
        },
        updateCurrentlanguage({ commit }, currentlanguage) {
            commit('setCurrentlanguage', currentlanguage);
        },
        updateChargestatusdata({ commit }, chargestatusdata) {
            commit('setChargestatusdata', chargestatusdata);
        },
        removeChargestatusdata({ commit }) {
            commit('clearChargestatusdata');
        },
    },
    getters: {
        getToken: state => state.token,
        getFid: state => state.fid,
        getUser: state => state.user,
        getChargedata: state => state.chargedata,
        getChargestatusdata: state => state.chargestatusdata,
        getPaydata: state => state.paydata,
        getTransdata: state => state.transdata,
        getTranslations: state => state.translations,
        getCurrentlanguage: state => state.currentlanguage,
    }
});

export default store;