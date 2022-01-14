/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

import React, { useState } from "react";

import { Card, Col, Container, Nav, Row} from "react-bootstrap";

export const Docs = (props) => {

    const [card, setCard] = useState("#purpose");
    console.log("Docs: " + props.redeemToken);
    return(
        <>
        {props.redeemToken?
            <h5 className="card-title">Please sign-in to complete signup.</h5>           
            :
        props.error?
            <div>
                <h5 className="card-title">You do not own or belong to any tenant.</h5>
                <h5> Please create one or get invited to a tenant owned by someone else to complete the signin.</h5>      
            </div>
        :       
            <Container className="text-justify">
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
            }         
    </>)
}