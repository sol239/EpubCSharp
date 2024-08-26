from flask import Flask, request, jsonify
import argostranslate.package
import argostranslate.translate
import threading
import os
import signal

app = Flask(__name__)

# Dictionary to store initialization status
initialization_statuses = {}

def initialize(source_language, target_language):
    """
    Initializes the Argos Translate package for the given language pair.
    """
    from_code = source_language
    to_code = target_language   

    try:
        argostranslate.package.update_package_index()
        available_packages = argostranslate.package.get_available_packages()
        package_to_install = next(
            filter(
                lambda x: x.from_code == from_code and x.to_code == to_code, available_packages
            )
        )
        argostranslate.package.install_from_path(package_to_install.download())
        initialization_statuses[(source_language, target_language)] = "Translation package initialized successfully."
    except Exception as e:
        initialization_statuses[(source_language, target_language)] = f"Initialization failed: {str(e)}"

@app.route('/initialize', methods=['POST'])
def init():
    """
    Handles initialization requests.
    """
    data = request.get_json()

    if not data or 'source_language' not in data or 'target_language' not in data:
        return jsonify({'error': 'Invalid input'}), 400

    source_language = data['source_language']
    target_language = data['target_language']

    # Start initialization in a separate thread
    threading.Thread(target=initialize, args=(source_language, target_language)).start()

    return jsonify({'message': 'Initialization started'}), 202

@app.route('/initialize/status', methods=['GET'])
def init_status():
    """
    Returns the initialization status for the specified language pair.
    """
    source_language = request.args.get('source_language')
    target_language = request.args.get('target_language')

    if not source_language or not target_language:
        return jsonify({'error': 'Missing parameters'}), 400

    status = initialization_statuses.get((source_language, target_language), 'Initialization not started or completed yet.')
    return jsonify({'initialization_status': status})

@app.route('/translate', methods=['POST'])
def translate():
    """
    Handles translation requests.
    """
    data = request.get_json()

    if not data or 'text' not in data or 'source_language' not in data or 'target_language' not in data:
        return jsonify({'error': 'Invalid input'}), 400

    text = data['text']
    source_language = data['source_language']
    target_language = data['target_language']

    try:
        # Perform the translation
        translated_text = argostranslate.translate.translate(text, source_language, target_language)

        return jsonify({'translated_text': translated_text}), 200

    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/shutdown', methods=['POST'])
def shutdown():
    """
    Shuts down the Flask application.
    """
    shutdown_function = request.environ.get('werkzeug.server.shutdown')
    if shutdown_function is None:
        return jsonify({'error': 'Not running with the Werkzeug Server'}), 500
    shutdown_function()
    return jsonify({'message': 'Server shutting down...'}), 200

if __name__ == "__main__":
    app.run(host="127.0.0.1", port=5000, debug=True)
