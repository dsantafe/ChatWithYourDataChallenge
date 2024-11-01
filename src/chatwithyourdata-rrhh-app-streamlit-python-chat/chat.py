import streamlit as st
import requests
import pandas as pd
from PIL import Image
import base64

BASE_URL = "https://chat-with-your-data-api.azurewebsites.net"

# CSS personalizado para una interfaz moderna
def apply_custom_style():
    st.markdown("""
        <style>
        /* Estilos generales */
        .stApp {
            background: linear-gradient(to bottom right, #1A202C, #2D3748);
        }
        
        /* Contenedor principal */
        .main-container {
            background: rgba(45, 55, 72, 0.25);
            backdrop-filter: blur(12px);
            border-radius: 16px;
            padding: 2rem;
            margin: 1rem;
        }
        
        /* Título principal */
        .main-title {
            background: linear-gradient(45deg, #00CED1, #4FD1C5);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            font-size: 2.2rem;
            font-weight: bold;
            text-align: center;
            padding: 1rem 0;
        }
        
        /* Mensajes del chat */
        .stChatMessage {
            background: rgba(45, 55, 72, 0.4);
            border-radius: 12px;
            padding: 1rem;
            margin: 0.5rem 0;
            border: 1px solid rgba(255, 255, 255, 0.1);
        }
        
        /* Botones */
        .stButton>button {
            background: linear-gradient(45deg, #00CED1, #4FD1C5);
            color: white;
            border: none;
            border-radius: 10px;
            padding: 0.75rem 2rem;
            font-weight: bold;
            transition: all 0.3s ease;
        }
        
        .stButton>button:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(0, 206, 209, 0.3);
        }
        
        /* Sidebar */
        .css-1d391kg {
            background-color: rgba(45, 55, 72, 0.5);
        }
        
        /* Inputs */
        .stTextInput input {
            background-color: rgba(45, 55, 72, 0.5);
            border: 1px solid #4A5568;
            border-radius: 10px;
            color: white;
            padding: 0.75rem;
        }
        
        /* Tablas */
        .dataframe {
            background: rgba(45, 55, 72, 0.3);
            border-radius: 8px;
            border: 1px solid rgba(255, 255, 255, 0.1);
        }
        
        .dataframe th {
            background: rgba(0, 206, 209, 0.1);
            color: #00CED1;
        }
        
        /* Código SQL */
        pre {
            background: rgba(45, 55, 72, 0.5) !important;
            border-radius: 8px !important;
            border: 1px solid rgba(255, 255, 255, 0.1) !important;
        }
        
        /* Stats en Sidebar */
        .stat-container {
            background: rgba(45, 55, 72, 0.3);
            border-radius: 10px;
            padding: 1rem;
            margin: 0.5rem 0;
            border: 1px solid rgba(255, 255, 255, 0.1);
        }
        
        .stat-value {
            font-size: 1.2rem;
            color: #00CED1;
            font-weight: bold;
        }
        
        /* Animaciones */
        @keyframes fadeIn {
            from {opacity: 0; transform: translateY(10px);}
            to {opacity: 1; transform: translateY(0);}
        }
        
        .fade-in {
            animation: fadeIn 0.5s ease-out forwards;
        }
        </style>
    """, unsafe_allow_html=True)

def image_to_base64(image):
    import io
    import base64
    buf = io.BytesIO()
    image.save(buf, format='PNG')
    byte_data = buf.getvalue()
    return base64.b64encode(byte_data).decode()

def chat():
    apply_custom_style()

    # Inicialización del estado de la sesión
    if 'initialized' not in st.session_state:
        st.session_state.total_cost = 0
        st.session_state.initialized = True
        st.session_state.prompt = None
        st.session_state.db_type = None
        st.session_state.tokens_used = None
        st.session_state.retail_price = None
        st.session_state.show_buttons = False
        st.session_state.process_query = False
        st.session_state.messages = [
            {"role": "assistant", "content": "Hi! 👋 How can I help you today?"}
        ]

    # Sidebar mejorado
    with st.sidebar:
        image = Image.open("assets/DataVerse.png")
        st.markdown(
            f"""
            <div style="text-align: center;" class="fade-in">
                <img src="data:image/png;base64,{image_to_base64(image)}" alt="DataVerse Logo" width="200">
            </div>
            """,
            unsafe_allow_html=True
        )

        st.markdown(f"""
            <div class="stat-container fade-in">
                <h3>Welcome, {st.session_state.username}! 👤</h3>
                <div class="stat-value">{st.session_state.tokens_available} tokens</div>
                <small>Available tokens</small>
            </div>
            
            <div class="stat-container fade-in">
                <h4>Conversation Cost</h4>
                <div class="stat-value">${st.session_state.total_cost:.4f}</div>
                <small>Total USD</small>
            </div>
        """, unsafe_allow_html=True)

    # Contenedor principal
    st.markdown('<div class="main-container">', unsafe_allow_html=True)

    # Título principal
    st.markdown('<h1 class="main-title fade-in">Chat with your data through DataVerse</h1>', unsafe_allow_html=True)

    # Chat container
    chat_container = st.container()
    with chat_container:
        for message in st.session_state.messages:
            with st.chat_message(message["role"]):
                st.markdown(message["content"])
                if "sql" in message:
                    st.code(message["sql"], language="sql")
                if "table" in message:
                    st.write('Your data result:')
                    st.table(message["table"])

    # Input del usuario
    prompt = st.chat_input("Tell me, what is your question?")

    # Lógica de procesamiento del prompt
    if prompt and prompt != st.session_state.prompt:
        st.session_state.prompt = prompt
        st.session_state.show_buttons = False
        st.session_state.process_query = False
        st.session_state.messages.append({"role": "user", "content": prompt})

        db_type, tokens_used, retail_price = get_token_comsuption(prompt)

        if db_type and tokens_used and retail_price:
            st.session_state.db_type = db_type
            st.session_state.tokens_used = tokens_used
            st.session_state.retail_price = retail_price
            st.session_state.show_buttons = True

            token_message = (
                f"The approximate number of tokens that you are going to use are: {tokens_used} with a price of: ${retail_price:.4f} dollars.  \n"
                f"Do you want to make the request?"
            )
            st.session_state.messages.append({"role": "assistant", "content": token_message})

            st.rerun()

    # Botones de confirmación
    if st.session_state.show_buttons and not st.session_state.process_query:
        col1, col2 = st.columns(2)
        with col1:
            if st.button("✓ Confirm", key="accept_button", use_container_width=True):
                st.session_state.process_query = True
                st.session_state.show_buttons = False
                st.rerun()
        with col2:
            if st.button("✗ Cancel", key="cancel_button", use_container_width=True):
                st.session_state.show_buttons = False
                st.session_state.prompt = None
                st.session_state.messages.append({"role": "assistant", "content": "Request annulment."})
                st.rerun()

    # Procesamiento de la consulta
    if st.session_state.process_query and st.session_state.prompt:
        if st.session_state.tokens_available >= st.session_state.tokens_used:
            query = generate_sql_query(st.session_state.prompt, st.session_state.db_type)

            if query:
                results = execute_sql_query(query=query, db_type=st.session_state.db_type)

                if results and len(results) > 0:
                    header = results[0].split(" | ")
                    data = [row.split(" | ") for row in results[1:]]
                    df = pd.DataFrame(data, columns=header)

                    st.session_state.messages.append({
                        "role": "assistant",
                        "content": "SQL query generated successfully:",
                        "sql": query,
                        "table": df
                    })
                else:
                    st.session_state.messages.append({
                        "role": "assistant",
                        "content": "No results found"
                    })
        else:
            st.session_state.messages.append({
                "role": "assistant",
                "content": "Insufficient tokens available to make the request"
            })

        st.session_state.process_query = False
        st.session_state.prompt = None
        st.session_state.show_buttons = False
        st.rerun()

    st.markdown('</div>', unsafe_allow_html=True)
def get_token_comsuption(prompt):
    data = {
        "naturalLanguageQuery": prompt,
        "userId": st.session_state.id
    }

    url = f"{BASE_URL}/api/Query/GetTokensConsumption"
    response = requests.post(url, json=data)
    
    if response.status_code == 200:
        data = response.json().get("data")
        db_type = data.get("selectedDatabaseType")
        tokens_used = data.get("tokenCount")
        retail_price = data.get("retailPrice")
        return db_type, tokens_used, retail_price
    elif response.status_code == 400:
        st.session_state.messages.append({
                "role": "assistant",
                "content": "No se encontro una base de datos que cumple con su consulta"
            })
        return None, None, None
    else:
        st.error("Error to get token consumption")
        return None, None, None
    
def generate_sql_query(prompt, db_type):
    url = f"{BASE_URL}/api/Query/GenerateSqlQuery"
    response = requests.post(url, json={
        "naturalLanguageQuery": prompt,
        "selectedDatabaseType": db_type,
        "userId": st.session_state.id
    })
    
    if response.status_code == 200:
        data = response.json().get("data", {})
        generated_sql_query = data.get("generatedSqlQuery")
        token_count_inp = data.get("tokenCountInp", 0)
        token_count_outp = data.get("tokenCountOutp", 0)
        retail_price = round(data.get("retailPrice", 0), 4)
        
        tokens_consumed = token_count_inp + token_count_outp
        st.session_state.tokens_available -= tokens_consumed
        st.session_state.total_cost += retail_price

        # Agregar información de tokens al historial
        token_info = (
            f"Tokens input: {token_count_inp}  \n"  # Usar dos espacios para un salto de línea
            f"Tokens output: {token_count_outp}  \n"
            f"Tokens consumed: {tokens_consumed}  \n"
            f"Retail price USD: {retail_price}"
        )
        st.session_state.messages.append({
            "role": "assistant",
            "content": token_info
        })
        
        return generated_sql_query
    else:
        st.error("Error generating sql query")
        return None

def execute_sql_query(query, db_type):
    url = f"{BASE_URL}/api/Query/ExecuteSqlQuery"
    response = requests.post(url, json={
        "generatedSqlQuery": query,
        "selectedDatabaseType": db_type
    })

    if response.status_code == 200:
        data = response.json().get("data", {})
        return data.get("results", [])
    else:
        st.error("Error executing sql query")
        return None
