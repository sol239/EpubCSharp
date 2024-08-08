from flask import Flask, request, jsonify
import argostranslate.package
import argostranslate.translate

app = Flask(__name__)

def initialize(source_language, target_language):
    from_code = source_language
    to_code = target_language

    # Download and install Argos Translate package
    argostranslate.package.update_package_index()
    available_packages = argostranslate.package.get_available_packages()
    package_to_install = next(
        filter(
            lambda x: x.from_code == from_code and x.to_code == to_code, available_packages
        )
    )
    argostranslate.package.install_from_path(package_to_install.download())

@app.route('/translate', methods=['POST'])
def translate():
    data = request.get_json()

    if not data or 'text' not in data or 'source_language' not in data or 'target_language' not in data:
        return jsonify({'error': 'Invalid input'}), 400

    text = data['text']
    source_language = data['source_language']
    target_language = data['target_language']

    try:
        # Initialize translation package
        initialize(source_language, target_language)

        # Perform translation
        translated_text = argostranslate.translate.translate(text, source_language, target_language)
        
        return jsonify({'translated_text': translated_text}), 200
    except Exception as e:
        return jsonify({'error': str(e)}), 500

if __name__ == "__main__":
    app.run(debug=True)
