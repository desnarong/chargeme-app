/* eslint-disable no-undef */
/** @type {import('tailwindcss').Config} */
export const content = [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
    "./node_modules/flowbite/**/*.js"
];
export const theme = {
    extend: {},
};
export const plugins = [
    require('flowbite/plugin')
];

