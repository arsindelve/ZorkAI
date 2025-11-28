/** @type {import('tailwindcss').Config} */
import daisyui from 'daisyui'

export default {
    daisyui: {
        themes: ["black"]
    },
    content: [
        './src/**/*.{html,js,jsx,tsx}',
        '../WebClients/packages/game-client-core/src/**/*.{js,jsx,ts,tsx}'
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
    plugins: [daisyui],
}