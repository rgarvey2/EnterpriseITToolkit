const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');

module.exports = (env, argv) => {
    const isProduction = argv.mode === 'production';
    
    return {
        mode: isProduction ? 'production' : 'development',
        entry: {
            main: './Web/wwwroot/js/site.js',
            secureAuth: './Web/wwwroot/js/secure-auth.js',
            styles: './Web/wwwroot/css/site.css'
        },
        output: {
            path: path.resolve(__dirname, 'Web/wwwroot/dist'),
            filename: isProduction ? '[name].[contenthash].js' : '[name].js',
            clean: true,
            publicPath: '/dist/'
        },
        module: {
            rules: [
                {
                    test: /\.css$/,
                    use: ['style-loader', 'css-loader']
                }
            ]
        },
        plugins: [
            new HtmlWebpackPlugin({
                template: './Web/wwwroot/index.html',
                filename: 'index.html',
                inject: true,
                minify: isProduction ? {
                    removeComments: true,
                    collapseWhitespace: true,
                    removeRedundantAttributes: true,
                    useShortDoctype: true,
                    removeEmptyAttributes: true,
                    removeStyleLinkTypeAttributes: true,
                    keepClosingSlash: true,
                    minifyJS: true,
                    minifyCSS: true,
                    minifyURLs: true
                } : false
            })
        ],
        resolve: {
            extensions: ['.js', '.css'],
            alias: {
                '@': path.resolve(__dirname, 'Web/wwwroot')
            }
        },
        devtool: isProduction ? false : 'eval-source-map'
    };
};
