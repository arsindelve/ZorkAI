/** @type {import('tailwindcss').Config} */
import daisyui from 'daisyui'

export default {
    daisyui: {
        themes: ["black"]
    },
    content: [
        './src/**/*.{html,js,jsx,tsx}',
        // Shared UI package (e.g. <Compass>) lives outside ./src; scan it too or
        // Tailwind purges classes used only there (the rose's w-36/lg:w-44 collapse).
        '../shared-web-types/src/**/*.{html,js,jsx,tsx}'
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