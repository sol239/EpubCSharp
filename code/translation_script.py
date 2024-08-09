from flask import Flask, request, jsonify
import os

# argos imports
import argostranslate.package
import argostranslate.translate
import argostranslate.settings

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

    print("Flask server initialized!")

@app.route('/translate', methods=['POST'])
def translate():
    
    # print("Available packages:")
    # print(argospm.get_available_packages())
    # [Albanian -> English, Arabic -> English, Azerbaijani -> English, Bengali -> English, ...

    # print("Installed packages:")
    # print(argostranslate.package.get_installed_packages())
    # [English -> German, English -> Spanish, Czech -> English, English -> Czech ...

    # print("Packages path:")
    # print(argostranslate.settings.get_packages_folder_path())
    # argostranslate.settings.get_path_of_packages()

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
    app.run(host="127.0.0.1", port=5000, debug=True)
