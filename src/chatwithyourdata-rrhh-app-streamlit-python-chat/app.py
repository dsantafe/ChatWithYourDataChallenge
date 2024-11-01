import streamlit as st
from authentication import authentication
from chat import chat

# Configuración de la página - DEBE SER LA PRIMERA LLAMADA A STREAMLIT
st.set_page_config(
    page_title="DataVerse",
    page_icon="📎",
    layout="wide",
    initial_sidebar_state="expanded"
)

# Manejo de la navegación
if 'username' not in st.session_state:
    authentication()
else:
    chat()