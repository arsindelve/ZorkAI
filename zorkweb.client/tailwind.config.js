/** @type {import('tailwindcss').Config} */
export default {
    content: [
        './src/**/*.{html,js,jsx,tsx}'
    ],
    mode: 'jit',
    variants: {
        extend: {

            backgroundRepeat: {
                'repeat': 'repeat',
            },
        },
    },
    theme: {
        extend: {},
    },
    plugins: [],
}

