import streamlit as st
import requests
from PIL import Image

# Configuración de la URL del endpoint
URL = "https://chat-with-your-data-api.azurewebsites.net"

def apply_authentication_style():
    st.markdown("""
        <style>
        .stApp {
            background: linear-gradient(to bottom right, #1A202C, #2D3748);
        }
        
        /* Contenedor de autenticación */
        .auth-container {
            background: rgba(45, 55, 72, 0.25);
            backdrop-filter: blur(12px);
            border-radius: 16px;
            padding: 2rem;
            margin: 0 auto;
            max-width: 500px;
            text-align: center;
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
        }
        
        /* Título de autenticación */
        .auth-title {
            background: linear-gradient(45deg, #00CED1, #4FD1C5);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            font-size: 2rem;
            font-weight: bold;
            text-align: center;
            padding: 0.5rem 0;
            margin-bottom: 0.5rem;  /* Reducido para acercar el subtítulo */
        }

        /* Subtítulo */
        .auth-subtitle {
            background: linear-gradient(45deg, #00CED1, #4FD1C5);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            font-size: 1.3rem;
            font-weight: 500;
            text-align: center;
            margin-bottom: 0.5rem;
        }

        /* Texto pequeño */
        .auth-small-text {
            background: linear-gradient(45deg, #FFFFFF, #FFFFFF);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            font-size: 1.0rem;
            font-weight: normal;
            text-align: center;
            margin-bottom: 1.5rem;
            letter-spacing: 1px;
        }
        
        /* Inputs de autenticación */
        .stTextInput input {
            background-color: rgba(45, 55, 72, 0.5);
            border: 1px solid #4A5568;
            border-radius: 10px;
            color: white;
            padding: 1rem;
            width: 100%;
            margin-bottom: 1rem;
            font-size: 1.1rem;
            height: 3rem;
        }
        
        /* Botón de login */
        .stButton>button {
            background: linear-gradient(45deg, #00CED1, #4FD1C5);
            color: white;
            border: none;
            border-radius: 10px;
            padding: 1rem 2rem;
            font-weight: bold;
            font-size: 1.1rem;
            transition: all 0.3s ease;
            width: 100%;
            height: 3rem;
        }
        
        .stButton>button:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(0, 206, 209, 0.3);
        }
        
        /* Mensajes de estado */
        .stAlert {
            background: rgba(45, 55, 72, 0.4);
            border-radius: 10px;
            border: 1px solid rgba(255, 255, 255, 0.1);
            padding: 0.75rem;
            margin-top: 1rem;
        }
        
        /* Animaciones */
        @keyframes fadeIn {
            from {opacity: 0; transform: translateY(10px);}
            to {opacity: 1; transform: translateY(0);}
        }
        
        .fade-in {
            animation: fadeIn 0.5s ease-out forwards;
        }
        
        /* Logo container */
        .logo-container {
            margin-bottom: 1.5rem;
            display: flex;
            justify-content: center;
            align-items: center;
        }
        
        .logo-container img {
            max-width: 200px;
            height: auto;
            display: block;
            margin: 0 auto;
        }

        /* Ajustes para el formulario */
        .stForm {
            max-width: 100%;
        }

        /* Ocultar elementos innecesarios de Streamlit */
        #MainMenu {visibility: hidden;}
        footer {visibility: hidden;}
        </style>
    """, unsafe_allow_html=True)

def image_to_base64(image):
    import io
    import base64
    buf = io.BytesIO()
    image.save(buf, format='PNG')
    byte_data = buf.getvalue()
    return base64.b64encode(byte_data).decode()

def authentication():
    # Aplicar estilos personalizados
    apply_authentication_style()

    # Contenedor principal
    st.markdown('<div class="auth-container fade-in">', unsafe_allow_html=True)

    # Logo
    image = Image.open("assets/DataVerseLogo.png")
    st.markdown(
        f"""
        <div class="logo-container">
            <img src="data:image/png;base64,{image_to_base64(image)}" alt="DataVerse Logo">
        </div>
        """,
        unsafe_allow_html=True
    )

    # Título y subtítulos
    st.markdown('''
        <h1 class="auth-title">Chat with your data through DataVerse!</h1>
        <h2 class="auth-subtitle">Interactive & Efficient Chat</h2>
        <p class="auth-small-text">Ask • Analyze • Decide</p>
    ''', unsafe_allow_html=True)

    # Formulario de login
    with st.form("login_form"):
        username = st.text_input("Username", placeholder="Enter your username")
        password = st.text_input("Password", type='password', placeholder="Enter your password")
        submit_button = st.form_submit_button("Login")

        if submit_button:
            if username and password:
                credentials = {
                    'username': username,
                    'password': password
                }
                response = requests.post(URL + "/api/Account/Login", json=credentials)

                if response.status_code == 200:
                    data = response.json()
                    # Guardar datos en session_state
                    st.session_state.id = data.get('id')
                    st.session_state.username = username
                    st.session_state.password_hash = data.get('passwordHash')
                    st.session_state.tokens_available = data.get('tokensAvailable')

                    # Mostrar mensaje de éxito con estilo
                    st.markdown("""
                        <div class="stAlert success">
                            ✅ Login successful! Redirecting...
                        </div>
                    """, unsafe_allow_html=True)
                    st.rerun()
                else:
                    st.error("❌ Username or password is incorrect")
            else:
                st.warning("⚠️ Username and password are required")

    st.markdown('</div>', unsafe_allow_html=True)