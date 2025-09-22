const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const fs = require('fs');

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
                   filename: '[name].js',
                   clean: true,
                   publicPath: './'
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
            }),
            // Copy files manually without plugin
            {
                apply: (compiler) => {
                    compiler.hooks.afterEmit.tap('CopyFilesPlugin', (compilation) => {
                        const outputPath = compilation.outputOptions.path;
                        
                        // Copy sw.js
                        try {
                            fs.copyFileSync('./Web/wwwroot/sw.js', path.join(outputPath, 'sw.js'));
                        } catch (err) {
                            console.warn('Could not copy sw.js:', err.message);
                        }
                        
                        // Copy manifest.json
                        try {
                            fs.copyFileSync('./Web/wwwroot/manifest.json', path.join(outputPath, 'manifest.json'));
                        } catch (err) {
                            console.warn('Could not copy manifest.json:', err.message);
                        }
                    });
                }
            }
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
