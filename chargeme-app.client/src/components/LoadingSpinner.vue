<template>
    <div v-if="isLoading" class="fixed inset-0 bg-gray-500 bg-opacity-75 flex flex-col items-center justify-center z-50">
        <!-- โลโก้ -->
        <!--<img src="../assets/loading.png" alt="Logo" class="w-24 h-24 mb-4" />-->
        <!-- สัญลักษณ์การโหลด -->
        <div class="loader mt-4"></div>
        <!--<img src="../assets/loading.png" alt="Logo" class="w-24 h-24 mb-4" />-->
        <!-- ข้อความ -->
        <p class="text-xl text-white font-semibold">{{ translations.PAGE_LOADING_TEXT }}</p>
    </div>
</template>

<script>
    export default {
        props: {
            isLoading: {
                type: Boolean,
                default: false
            }
        },
        computed: {
            translations() {
                return this.$store.getters.getTranslations;
            },
        },
    };
</script>

<style scoped>
    .loader {
        border: 8px solid #f3f3f3;
        border-radius: 50%;
        border-top: 8px solid #071EAB;
        width: 60px;
        height: 60px;
        animation: spin 2s linear infinite;
    }

    @keyframes spin {
        0% {
            transform: rotate(0deg);
        }

        100% {
            transform: rotate(360deg);
        }
    }
</style>
