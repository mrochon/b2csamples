/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */
import React, { useState } from "react";

import { AuthenticatedTemplate, UnauthenticatedTemplate, useMsal } from "@azure/msal-react";

import { Navbar, Button, Card, Col, Container, Dropdown, DropdownButton, Nav, Row} from "react-bootstrap";

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
                    <DropdownButton variant="warning" className="ml-auto" drop="left" title="Sign Out">
                        <Dropdown.Item as="button" onClick={() => instance.logoutPopup({ postLogoutRedirectUri: "/", mainWindowRedirectUri: "/" })}>Sign out using Popup</Dropdown.Item>
                        <Dropdown.Item as="button" onClick={() => instance.logoutRedirect({ postLogoutRedirectUri: "/" })}>Sign out using Redirect</Dropdown.Item>
                    </DropdownButton>
                </div>
            </AuthenticatedTemplate>
            <UnauthenticatedTemplate>
                <div className="ml-auto">
                    <Button variant="warning" className="ml-auto" drop="left" onClick={() => 
                        instance.loginRedirect({ 
                            authority:b2cPolicies.authorities.newTenant.authority
                        })
                    }>Create new tenant</Button>
                </div>   
                <div className="ml-auto">                         
                    <Button variant="warning" className="ml-auto" onClick={() => instance.loginRedirect(loginRequest)}>Sign in</Button>  
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
    const [card, setCard] = useState("#purpose");

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
            <AuthenticatedTemplate>
                <footer>
                    <center> 
                        <a href="https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant" target="_blank"> Source</a>
                    </center>
                </footer> 
            </AuthenticatedTemplate>
            <UnauthenticatedTemplate>
                <Container>
                    <Row>
                        <Col />
                        <Col>
                            <Card style={{ width: '30rem' }}>
                                <Card.Header>
                                    <Nav variant="tabs" defaultActiveKey="#purpose" onSelect={(selected => setCard(selected))}>
                                        <Nav.Item>
                                            <Nav.Link href="#purpose">Purpose</Nav.Link>
                                        </Nav.Item>
                                        <Nav.Item>
                                            <Nav.Link href="#functionality">Functionality</Nav.Link>
                                        </Nav.Item>
                                        <Nav.Item>
                                            <Nav.Link href="#scenario">Scenario</Nav.Link>
                                        </Nav.Item>                                        
                                        <Nav.Item>
                                            <Nav.Link href="#resources">Resources</Nav.Link>
                                        </Nav.Item>
                                    </Nav>
                                </Card.Header>      
                                {card === "#purpose"?                                                           
                                <Card.Body>
                                    <Card.Subtitle className="mb-2 text-muted">B2C for SaaS apps for small businesses</Card.Subtitle>
                                    <Card.Text>
                                        <p>Demonstrates use of a single Azure B2C directory to provide user identity support for an application needing to partition
                                        its users into distinct groups (application tenants). A Software as a Service (SaaS) application commonly uses the concept of a <b>tenant</b> to group users from different organizations.</p>
                                        <p>For example, a SaaS 
                                        application may provide accounting
                                        services. Each accounting business/firm using the application is considered a tenant in the application. Since Azure AD itself is a SaaS application, to distinguish the term when
                                        applied to user partitioning implemented here, the term <b>application tenant</b> will be used here.</p>
                                        <p><i>Note that as implemented in this sample, users may indeed be <b>authenticated</b> in many other AADs or other IdP. However, an application using the functionality provided
                                        in this sample will generally not be aware of that. Information as to which <b>application tenant</b> a user belongs to, will be provided by the single B2C directory used here.</i></p>
                                    </Card.Text>
                                </Card.Body>
                                :
                                card === "#scenario"?
                                <Card.Body>
                                    <Card.Text>
                                        <p>You own a small accounting business. You would like to use an application provided by Contoso. The application is deployed in the cloud. To use it, you need to navigate your browser (or install Contoso' mobile app)
                                        and sign up. Once you have signed up (and probably paid for the use of the software) you would like to enable your other coleagues in the firm to sign up as well so that you can jointly work on the data your 
                                        firm manages. Obviously, when you sign in to the software, it needs to know which company (application tenant) you own or belong to so it can separate your firm's data from data used by other firms.</p>

                                        <p>To implement this scenario:</p>
                                         <ol>
                                            <li>Select the <i>Create new tenant</i> button</li>
                                            <li>If this is your first use, either select <i>Signup</i>, or...</li>
                                            <li>...select <i>work or school</i> account if you have an AAD account, or...</li>
                                            <li>...select <i>Google</i> to use your gmail account </li>
                                            <li>when prompted enter the name of the application tenant, e.g. your company's name and description</li>
                                            <li>You will then be redirected back to this application and have several options visible on the page: <i>Claims, Members, etc. </i></li>
                                            <li>Select the <i>Invite someone</i> option</li>
                                            <li>Enter the person's email and select <i>Invite</i></li>
                                            <li>Copy the new url <i>Invitation</i>link and send it to the invitee</li>
                                            <li>Using this link, they will be able to signup/in as well and become another user in your tenant</li>
                                        </ol>                                     
                                    </Card.Text>
                                </Card.Body>   
                                :                             
                                card === "#functionality"?
                                <Card.Body>
                                    <Card.Text>
                                        <ul>
                                            <li>Signin/up and create a new application tenant</li>
                                            <li>Invite other users to your tenant using their email address</li>
                                            <li>Redeem invitation and join a tenant</li>
                                            <li>View list of all members of a tenant</li>
                                            <li>Switch tenants (a user may create or join any number of tenant)</li>
                                            <li>Create a local account using your own email and a password, or...</li>
                                            <li>...use your AAD work or school identity to signin/up, or...</li>
                                            <li>...use your gmail identity to signin/up</li>                                            
                                            <li>automatically redirect an invited user to their AAD tenant for authentication</li>
                                            <li>Allow tenants created using AAD credentials to automatically accept to the application tenat any other user from the same organization</li>
                                            <li>Select to edit your B2C attributes as part of signin</li>
                                        </ul>
                                    </Card.Text>
                                </Card.Body>
                                :
                                <Card.Body>
                                    <Card.Text>
                                        <ul>
                                            <li><a href="https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant" target="_blank" rel="noopener">Source code</a></li>
                                            <li><a href="mailto:sailingrock@live.com" target="_blank" rel="noopener">My email</a></li>
                                        </ul>
                                    </Card.Text>
                                </Card.Body>                                                  
                                }
                            </Card>   
                        </Col>   
                        <Col />      
                    </Row>                          
                </Container>      
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
                    prompt: "login",
                    extraQueryParameters: { tenant: tenant }
                })
            }>{tenant}</Dropdown.Item>
    );
    function currTenant(tenant) {
        return (tenant !== accounts[0].idTokenClaims.appTenantName);
    }
    if(accounts[0].idTokenClaims.allTenants.length > 1) {
        var title = `Current: ${accounts[0].idTokenClaims.appTenantName}`
        return (
                <DropdownButton variant="warning" className="ml-auto" drop="left" title={title}>{listTenants}</DropdownButton>
    )} else return null;
};




