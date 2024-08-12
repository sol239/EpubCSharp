from flask import Flask, request, jsonify
import os

# argos imports for translation
import argostranslate.package
import argostranslate.translate
import argostranslate.settings

app = Flask(__name__)

def initialize(source_language, target_language):
    """
    Initializes the Argos Translate package for the given language pair.

    Downloads and installs the translation package corresponding to the
    specified source and target languages.

    Args:
        source_language (str): The language code for the source language.
        target_language (str): The language code for the target language.
    """
    from_code = source_language
    to_code = target_language   

    # Update the package index to get the latest available packages
    argostranslate.package.update_package_index()
    
    # Get the list of available translation packages
    available_packages = argostranslate.package.get_available_packages()
    
    # Find the package matching the source and target language codes
    package_to_install = next(
        filter(
            lambda x: x.from_code == from_code and x.to_code == to_code, available_packages
        )
    )
    
    # Download and install the selected package
    argostranslate.package.install_from_path(package_to_install.download())

    print("Flask server initialized!")

@app.route('/translate', methods=['POST'])
def translate():
    """
    Handles translation requests.

    Receives a JSON payload containing the text to be translated, the source language,
    and the target language. Initializes the translation package for the specified
    languages and performs the translation.

    Returns:
        Response: JSON response with the translated text or an error message.
    """
    # Extract data from the incoming JSON request
    data = request.get_json()

    # Validate the input data
    if not data or 'text' not in data or 'source_language' not in data or 'target_language' not in data:
        return jsonify({'error': 'Invalid input'}), 400

    text = data['text']
    source_language = data['source_language']
    target_language = data['target_language']

    try:
        # Initialize translation package for the specified languages
        initialize(source_language, target_language)

        # Perform the translation
        translated_text = argostranslate.translate.translate(text, source_language, target_language)
        
        # Return the translated text in JSON format
        return jsonify({'translated_text': translated_text}), 200
    except Exception as e:
        # Return an error message if something goes wrong
        return jsonify({'error': str(e)}), 500

if __name__ == "__main__":
    # Run the Flask web server
    app.run(host="127.0.0.1", port=5000, debug=True)
