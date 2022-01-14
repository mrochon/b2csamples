import React, { useState, useEffect } from "react";

import { MsalProvider, AuthenticatedTemplate, UnauthenticatedTemplate, useMsal } from "@azure/msal-react";
import { EventType, InteractionType } from "@azure/msal-browser";

import { loginRequest, b2cPolicies } from "./authConfig";
import { PageLayout, IdTokenClaims, InviteMember } from "./ui.jsx";
import { Tenant } from "./tenant.jsx";
import { Docs } from "./docs.jsx";

import "./styles/App.css";

/**
 * Most applications will need to conditionally render certain components based on whether a user is signed in or not. 
 * msal-react provides 2 easy ways to do this. AuthenticatedTemplate and UnauthenticatedTemplate components will 
 * only render their children if a user is authenticated or unauthenticated, respectively.
 */
const MainContent = () => {

    const { instance } = useMsal();
    const [redeemToken, setRedeemToken ] = useState(null);
    const [loginError, setLoginError ] = useState(null);

    /**
     * Using the event API, you can register an event callback that will do something when an event is emitted. 
     * When registering an event callback in a react component you will need to make sure you do 2 things.
     * 1) The callback is registered only once
     * 2) The callback is unregistered before the component unmounts.
     * For more, visit: https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-react/docs/events.md
     */
    useEffect(() => {
        
        const queryParams = new URLSearchParams(window.location.search);
        const id = queryParams.get('id_token');
        const domain_hint = queryParams.get('domain_hint');
        const tenant = queryParams.get('tenant');
        const login_hint = queryParams.get('login_hint');        
        console.log("id_token:" + id);
        setRedeemToken(id);

        const callbackId = instance.addEventCallback((event) => {
            if (event.eventType === EventType.LOGIN_FAILURE) {
                console.log("sign in failed:" + event.error.errorMessage);
                if (event.error && event.error.errorMessage.indexOf("AADB2C90118") > -1) {
                    if (event.interactionType === InteractionType.Redirect) {
                        instance.loginRedirect(b2cPolicies.authorities.forgotPassword);
                    } else if (event.interactionType === InteractionType.Popup) {
                        instance.loginPopup(b2cPolicies.authorities.forgotPassword)
                            .catch(e => {
                                return;
                            });
                    }
                } else {
                    setLoginError("No tenants!")
                }
            }

            if (event.eventType === EventType.LOGIN_SUCCESS) {
                if (event?.payload) {
                    /**
                     * We need to reject id tokens that were not issued with the default sign-in policy.
                     * "acr" claim in the token tells us what policy is used (NOTE: for new policies (v2.0), use "tfp" instead of "acr").
                     * To learn more about B2C tokens, visit https://docs.microsoft.com/en-us/azure/active-directory-b2c/tokens-overview
                     */
                    /*if (event.payload.idTokenClaims["acr"] === b2cPolicies.names.forgotPassword) {
                        window.alert("Password has been reset successfully. \nPlease sign-in with your new password");
                        return instance.logout();
                    }*/
                }
            }
        });
        if(domain_hint || login_hint || tenant) {
            instance.loginRedirect({
                scopes: loginRequest.scopes, 
                loginHint: login_hint,
                extraQueryParameters : {
                    domain_hint: domain_hint,
                    tenant: tenant
                }})          
        }

        return () => {
            if (callbackId) {
                instance.removeEventCallback(callbackId);
            }
        };
    }, []);

    return (
        <div className="App">
            <AuthenticatedTemplate>
                <Tenant />
            </AuthenticatedTemplate>

            <UnauthenticatedTemplate>
                <Docs redeemToken={redeemToken} error={loginError} />
            </UnauthenticatedTemplate>
        </div>
    );
};

/**
 * msal-react is built on the React context API and all parts of your app that require authentication must be 
 * wrapped in the MsalProvider component. You will first need to initialize an instance of PublicClientApplication 
 * then pass this to MsalProvider as a prop. All components underneath MsalProvider will have access to the 
 * PublicClientApplication instance via context as well as all hooks and components provided by msal-react. For more,
 * visit: https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-react/docs/getting-started.md
 */
export default function App({msalInstance}) {

    return (
        <MsalProvider instance={msalInstance}>
            <PageLayout>
                <MainContent />
            </PageLayout>
        </MsalProvider>
    );
}
