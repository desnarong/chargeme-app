<template>
    <div class="relative pl-5">
        <!-- ปุ่มเปิดเมนู -->
        <button
                @click="toggleMenu"
                ref="menuButton"
                class="p-2 z-50 text-gray-400 hover:text-gray-600 transition-colors duration-200 rounded-full">
            <i class="fas fa-bars text-[22px]"></i>
        </button>

        <!-- Overlay และ Sidebar -->
        <transition name="fade">
            <div v-if="isOpen" class="fixed inset-0 bg-black bg-opacity-50 z-40" @click.self="closeMenu">
                <transition name="slide">
                    <div v-if="isOpen"
                         class="fixed left-0 top-0 w-72 h-full bg-white shadow-xl p-6 z-50 overflow-y-auto">
                        <!-- ปุ่มปิดเมนู -->
                        <button @click="closeMenu"
                                class="absolute top-4 right-4 p-2 text-gray-500 hover:text-red-500 transition-colors duration-200">
                            <i class="fas fa-times text-[20px]"></i>
                        </button>

                        <!-- รูปโปรไฟล์ -->
                        <div class="flex flex-col items-center mt-4">
                            <div class="w-20 h-20 rounded-full border-4 border-blue-100 overflow-hidden">
                                <img :src="userImage" alt="User Image" class="w-full h-full object-cover" />
                            </div>
                            <h6 class="mt-3 text-lg font-semibold text-gray-800">{{ user.fName }} {{ user.fLastname }}</h6>
                        </div>

                        <!-- รายการเมนู -->
                        <ul class="mt-8 space-y-2">
                            <li class="p-3 rounded-lg hover:bg-blue-50 text-gray-700 hover:text-blue-600 cursor-pointer flex items-center gap-3 transition-colors duration-200"
                                @click="goTohome">
                                <i class="fas fa-home text-[18px]"></i>
                                <span>{{ translations.MENU_HOME }}</span>
                            </li>
                            <li class="p-3 rounded-lg hover:bg-blue-50 text-gray-700 hover:text-blue-600 cursor-pointer flex items-center gap-3 transition-colors duration-200"
                                @click="goToprofile">
                                <i class="fas fa-user text-[18px]"></i>
                                <span>{{ translations.MENU_USERPROFILE }}</span>
                            </li>
                            <li class="p-3 rounded-lg hover:bg-blue-50 text-gray-700 hover:text-blue-600 cursor-pointer flex items-center gap-3 transition-colors duration-200"
                                @click="goTotrans">
                                <i class="fas fa-exchange-alt text-[18px]"></i>
                                <span>{{ translations.MENU_HISTORY }}</span>
                            </li>
                            <li class="p-3 rounded-lg hover:bg-blue-50 text-gray-700 hover:text-blue-600 cursor-pointer flex items-center gap-3 transition-colors duration-200"
                                @click="goTotrans">
                                <i class="fas fa-map-marker-alt text-[18px]"></i>
                                <span>{{ translations.MENU_MAP }}</span>
                            </li>
                            <li class="p-3 rounded-lg hover:bg-red-50 text-red-600 hover:text-red-700 cursor-pointer flex items-center gap-3 transition-colors duration-200"
                                @click="goTologout">
                                <i class="fas fa-sign-out-alt text-[18px]"></i>
                                <span>{{ translations.MENU_LOGOUT }}</span>
                            </li>
                        </ul>
                    </div>
                </transition>
            </div>
        </transition>
    </div>
</template>

<script>
    import axios from '@/services/api.js';

    export default {
        data() {
            return {
                isOpen: false,
                userImage: "",
            };
        },
        computed: {
            user() {
                return this.$store.getters.getUser;
            },
            translations() {
                return this.$store.getters.getTranslations;
            },
            token() {
                return this.$store.getters.getToken;
            },
        },
        methods: {
            toggleMenu() {
                this.isOpen = !this.isOpen;
            },
            closeMenu() {
                this.isOpen = false;
            },
            async goTohome() {
                this.closeMenu();
                this.$router.push({ name: 'home' });
            },
            async goToprofile() {
                this.closeMenu();
                this.$router.push({ name: 'profile' });
            },
            async goTotrans() {
                this.closeMenu();
                this.$router.push({ name: 'trans' });
            },
            loadUserImage() {
                this.userImage = this.user.fImage
                    ? window.location.origin + this.user.fImage
                    : window.location.origin + '/userimage.png';
            },
            async goTologout() {
                try {
                    const response = await axios.post(`/auth/logout`, {}, {
                        headers: { 'Authorization': `Bearer ${this.token}` }
                    });
                    if (response.status === 200 && response.data === 'success') {
                        this.$store.dispatch('removeToken');
                        this.$store.dispatch('removeUser');
                        await this.$router.push({ name: 'home' });
                        this.closeMenu();
                        location.reload();
                    } else {
                        console.error('Logout failed:', response);
                    }
                } catch (error) {
                    console.error('Error during logout:', error);
                }
            },
            handleClickOutside(event) {
                if (
                    this.isOpen &&
                    this.$refs.menuButton &&
                    !this.$refs.menuButton.contains(event.target) &&
                    !event.target.closest('.fixed.left-0')
                ) {
                    this.closeMenu();
                }
            }
        },
        beforeUnmount() {
            document.removeEventListener("mousedown", this.handleClickOutside);
        },
        mounted() {
            document.addEventListener("mousedown", this.handleClickOutside);
            this.loadUserImage();
        },
        watch: {
            'user.fImage': {
                immediate: true,
                handler() {
                    this.loadUserImage();
                }
            },
            $route() {
                this.isOpen = false;
            }
        },
    };
</script>

<style scoped>
    /* Animation for overlay */
    .fade-enter-active, .fade-leave-active {
        transition: opacity 0.3s ease;
    }

    .fade-enter-from, .fade-leave-to {
        opacity: 0;
    }

    /* Animation for sidebar */
    .slide-enter-active, .slide-leave-active {
        transition: transform 0.3s ease;
    }

    .slide-enter-from, .slide-leave-to {
        transform: translateX(-100%);
    }
</style>