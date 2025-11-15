import { ApolloClient, InMemoryCache, createHttpLink } from '@apollo/client';

const GRAPHQL_URI = import.meta.env.VITE_GRAPHQL_URI || 'http://localhost:5126/graphql';

const httpLink = createHttpLink({
  uri: GRAPHQL_URI,
});

export const apolloClient = new ApolloClient({
  link: httpLink,
  cache: new InMemoryCache(),
  defaultOptions: {
    watchQuery: {
      errorPolicy: 'all',
    },
    query: {
      errorPolicy: 'all',
    },
  },
});

