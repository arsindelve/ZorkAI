/** @type {import('tailwindcss').Config} */
export default {
    daisyui: {
      themes: ["black"]  
    },
    content: [
        './src/**/*.{html,js,jsx,tsx}'
    ],
    mode: 'jit',
    theme: {
        extend: {
            fontFamily: {
                poppins: ["Poppins", "sans-serif"],
                platypi: ["Platypi", "serif"],
                mono: ["Roboto Mono", "serif"],
            },
        },
    },
    variants: {
        extend: {

            backgroundRepeat: {
                'repeat': 'repeat',
            },
        },
    },

    plugins: [require("daisyui")],
}

