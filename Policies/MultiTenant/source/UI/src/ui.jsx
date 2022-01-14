/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */
import React from "react";

import { AuthenticatedTemplate, UnauthenticatedTemplate, useMsal } from "@azure/msal-react";

import { Navbar, Button, Dropdown, DropdownButton } from "react-bootstrap";

import { loginRequest, b2cPolicies, deployment } from "./authConfig";

const NavigationBar = () => {

    /**
     * useMsal is hook that returns the PublicClientApplication instance, 
     * an array of all accounts currently signed in and an inProgress value 
     * that tells you what msal is currently doing. For more, visit:
     * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-react/docs/hooks.md
     */
    const { instance } = useMsal();

    return (
        <>
            <AuthenticatedTemplate>
                <div className="ml-auto">
                    <SwitchTenant/>
                </div>                  
                <div className="ml-auto">
                    <Button variant="warning" className="ml-auto" onClick={() => instance.logoutRedirect({ postLogoutRedirectUri: "/" })}>Sign out</Button>                      
                    {/*<DropdownButton variant="warning" className="ml-auto" drop="left" title="Sign Out">
                        <Dropdown.Item as="button" onClick={() => instance.logoutPopup({ postLogoutRedirectUri: "/", mainWindowRedirectUri: "/" })}>Sign out using Popup</Dropdown.Item>
                        <Dropdown.Item as="button" onClick={() => instance.logoutRedirect({ postLogoutRedirectUri: "/" })}>Sign out using Redirect</Dropdown.Item>
                    </DropdownButton>*/}
                </div>
            </AuthenticatedTemplate>
            <UnauthenticatedTemplate>
                <div className="ml-auto">
                    <Button variant="warning" className="ml-auto" drop="left" onClick={() => 
                        instance.loginRedirect({ 
                            authority:b2cPolicies.authorities.newTenant.authority,
                            scopes: loginRequest.scopes                           
                        })
                    }>Create new tenant</Button>
                </div>   
                <div className="ml-auto">                         
                    <Button variant="warning" className="ml-auto" onClick={() => 
                        instance.loginRedirect({
                            scopes: loginRequest.scopes
                            })
                        }>Sign in</Button>  
                </div>   
                {/*              
                <DropdownButton variant="secondary" className="ml-auto" drop="left" title="Sign In">
                    <Dropdown.Item as="button" onClick={() => instance.loginPopup(loginRequest)}>Sign in using Popup</Dropdown.Item>
                    <Dropdown.Item as="button" onClick={() => instance.loginRedirect(loginRequest)}>Sign in using Redirect</Dropdown.Item>
                </DropdownButton>
                */}
            </UnauthenticatedTemplate>
        </>
    );
};

export const PageLayout = (props) => {
    return (
        <>
            <Navbar bg="primary" variant="dark">
                <a className="navbar-brand" href="/">B2C multi-tenant</a>
                <NavigationBar />
            </Navbar>
            <br />
            <h5><center>Welcome to the 'B2C as multi-tenant identity' sample</center></h5>
            <br />
            {props.children}
            <br />
            {/*<AuthenticatedTemplate>
                <footer>
                    <center> 
                        <a href="https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant" target="_blank"> Source</a>
                    </center>
                </footer> 
            </AuthenticatedTemplate>*/}
            <UnauthenticatedTemplate>

            </UnauthenticatedTemplate>
        </>
    );
};

export const SwitchTenant = () => {
    const { accounts, instance } = useMsal();  

    const listTenants = accounts[0].idTokenClaims.allTenants.filter(currTenant).map((tenant, ix) =>
        <Dropdown.Item as="button" key={ix} onClick={() => 
                instance.loginRedirect({ 
                    authority:b2cPolicies.authorities.signIn.authority,
                    scopes: ["openid", "profile", `https://${deployment.b2cTenantName}.onmicrosoft.com/mtrest/User.Invite`, `https://${deployment.b2cTenantName}.onmicrosoft.com/mtrest/User.ReadAll`],                    
                    account: accounts[0],
                    extraQueryParameters: { tenant: tenant }
                })
            }>{tenant}</Dropdown.Item>
    );
    function currTenant(tenant) {
        return (tenant != accounts[0].idTokenClaims.appTenantName.toUpperCase());
    }
    if(accounts[0].idTokenClaims.allTenants.length > 1) {
        var title = `${accounts[0].idTokenClaims.appTenantName}`
        return (
                <DropdownButton variant="warning" className="ml-auto" drop="left" title={title}>{listTenants}</DropdownButton>
    )} else return null;
};




