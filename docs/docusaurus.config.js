// @ts-check
// Note: type annotations allow type checking and IDEs autocompletion

const lightCodeTheme = require('prism-react-renderer/themes/github');
const darkCodeTheme = require('prism-react-renderer/themes/dracula');

/** @type {import('@docusaurus/types').Config} */
const config = {
    title: 'Revolt.Net',
    tagline: 'Revolt.Net Documentation',
    url: 'https://revolt.net.jan0660.dev',
    baseUrl: '/',
    onBrokenLinks: 'throw',
    onBrokenMarkdownLinks: 'warn',
    favicon: 'img/favicon.ico',
    organizationName: 'Jan0660', // Usually your GitHub org/user name.
    projectName: 'Taco', // Usually your repo name.

    presets: [
        [
            'classic',
            /** @type {import('@docusaurus/preset-classic').Options} */
            ({
                docs: {
                    sidebarPath: require.resolve('./sidebars.js'),
                    // Please change this to your repo.
                    // editUrl: 'https://github.com/facebook/docusaurus/tree/main/packages/create-docusaurus/templates/shared/',
                    routeBasePath: "/",
                },
                blog: false,
                theme: {
                    customCss: require.resolve('./src/css/custom.css'),
                },
            }),
        ],
    ],

    themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
        ({
            navbar: {
                title: 'Revolt.Net',
                logo: {
                    alt: 'My Site Logo',
                    src: 'img/logo.svg',
                },
                items: [
                    {
                        href: "/api",
                        label: "API Documentation",
                        position: "left",
                    },
                    {
                        href: "/api-commands",
                        label: "Commands API Documentation",
                        position: "left",
                    },
                    {
                        label: 'GitHub',
                        href: 'https://github.com/Jan0660/Taco/Revolt.Net',
                        position: "right",
                    },
                ],
            },
            prism: {
                theme: lightCodeTheme,
                darkTheme: darkCodeTheme,
                additionalLanguages: ["csharp"],
            },
            colorMode: {
                defaultMode: "dark",
            }
        }),

    plugins: [require.resolve('docusaurus-lunr-search')],
};


module.exports = config;
