/** @type {import('tailwindcss').Config} */
export default {
    content: [
        './src/**/*.{html,js,jsx,tsx}'
    ],
    mode: 'jit',
    variants: {
        extend: {
            borderColor: ['focus'],
            boxShadow: ['focus'],
            outline: ['focus'],
        },
    },
    theme: {
        extend: {},
    },
    plugins: [],
}

