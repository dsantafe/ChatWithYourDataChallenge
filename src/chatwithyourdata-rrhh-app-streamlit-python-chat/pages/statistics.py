import streamlit as st
import requests
import pandas as pd
import plotly.express as px

def statistics():
    # Page configuration
    st.title("📊 Token Usage Statistics")

    # Constants
    BASE_URL = "https://chat-with-your-data-api.azurewebsites.net"
    API_URL = f"{BASE_URL}/api/Query/GetTokensConsumptionHistory/a3c09d9b-4baf-4c8a-92f1-1d33cb65e4f8"

    def fetch_data():
        """Fetches consumption data from the API"""
        try:
            response = requests.get(API_URL)
            response.raise_for_status()
            data = response.json()

            if data["isSuccess"]:
                return data["data"]
            else:
                st.error(f"Error retrieving data: {data['message']}")
                return []
        except requests.exceptions.RequestException as e:
            st.error(f"Connection error: {str(e)}")
            return []

    def prepare_data(data):
        """Prepares data for visualization"""
        if not data:
            return None

        df = pd.DataFrame(data)
        df['date'] = pd.to_datetime(df['year'].astype(str) + '-' + df['month'].astype(str))
        df = df[['date', 'tokensConsumed']].groupby('date').sum().reset_index()
        return df

    # Main container
    with st.container():
        # Get and prepare data
        data = fetch_data()
        df = prepare_data(data)

        if df is not None and not df.empty:
            # Create Plotly chart
            fig = px.bar(
                df,
                x='date',
                y='tokensConsumed',
                title='Monthly Token Consumption',
                labels={
                    'date': 'Date',
                    'tokensConsumed': 'Tokens Consumed'
                }
            )

            # Customize chart layout
            fig.update_layout(
                plot_bgcolor='white',
                yaxis_gridcolor='lightgray',
                xaxis_gridcolor='lightgray',
                bargap=0.2
            )

            # Display chart
            st.plotly_chart(fig, use_container_width=True)

            # Display basic statistics
            col1, col2, col3 = st.columns(3)
            with col1:
                st.metric(
                    "Total Tokens",
                    f"{df['tokensConsumed'].sum():,.0f}"
                )
            with col2:
                st.metric(
                    "Monthly Average",
                    f"{df['tokensConsumed'].mean():,.0f}"
                )
            with col3:
                st.metric(
                    "Monthly Maximum",
                    f"{df['tokensConsumed'].max():,.0f}"
                )

            # Data table
            with st.expander("View detailed data"):
                st.dataframe(
                    df.rename(columns={
                        'date': 'Date',
                        'tokensConsumed': 'Tokens Consumed'
                    }).sort_values('Date', ascending=False),
                    use_container_width=True
                )
        else:
            st.info("No data available to display")

if __name__ == "__main__":
    if 'id' in st.session_state:
        statistics()
    else:
        st.title("Please log in to continue")